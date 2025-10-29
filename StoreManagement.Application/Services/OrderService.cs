using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using StoreManagement.Application.DTOs.Order;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;
using StoreManagement.Domain.Interfaces;

namespace StoreManagement.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    // Method to recalculate order total
    private async Task RecalculateOrderTotalAsync(Order order)
    {
        // ⭐ SỬA LỖI CỐT LÕI: Tính TotalAmount từ OrderItems (Tổng tiền hàng)
        // Đã thay đổi để dùng Quantity * Price, thay vì oi.Subtotal (giả định Subtotal chưa được cập nhật)
        var totalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price); 
        order.TotalAmount = totalAmount;

        // Recalculate discount if promotion exists
        if (order.PromoId.HasValue)
        {
            var promotion = await _promotionRepository.GetByIdAsync(order.PromoId.Value);
            if (promotion != null && order.TotalAmount.HasValue)
            {
                order.DiscountAmount = await CalculateDiscountAsync(promotion, order.TotalAmount.Value);
            }
            else
            {
                // Nếu PromoId tồn tại nhưng không tìm thấy khuyến mãi (hoặc TotalAmount null), set Discount = 0
                order.DiscountAmount = 0;
            }
        }
        else
        {
            // Nếu không có khuyến mãi, set Discount = 0
            order.DiscountAmount = 0;
        }

        // KHÔNG CẦN order.FinalAmount = ... vì trường này không tồn tại
    }

    // Helper method to calculate discount (giữ nguyên)
    private Task<decimal> CalculateDiscountAsync(Promotion promotion, decimal orderAmount)
    {
        decimal discount;
        if (promotion.DiscountType == DiscountType.Percent)
        {
            discount = orderAmount * (promotion.DiscountValue / 100);
        }
        else // Fixed
        {
            discount = Math.Min(promotion.DiscountValue, orderAmount);
        }
        return Task.FromResult(discount);
    }

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        IPromotionRepository promotionRepository,
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _promotionRepository = promotionRepository;
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // Order CRUD - Basic operations 
    public async Task<OrderResponse?> GetByIdAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return order != null ? _mapper.Map<OrderResponse>(order) : null;
    }

    public async Task<(IEnumerable<OrderResponse> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, int pageSize, OrderStatus? status = null, int? userId = null, int? customerId = null,
        string? sortBy = null, bool sortDesc = false)
    {
        // Build filter expression
        Expression<Func<Order, bool>>? filter = null;

        if (status.HasValue || userId.HasValue || customerId.HasValue)
        {
            filter = o =>
                (!status.HasValue || o.Status == status.Value) &&
                (!userId.HasValue || o.UserId == userId) &&
                (!customerId.HasValue || o.CustomerId == customerId);
        }

        // Build order expression with whitelist and stable tie-breaker by OrderId
        Expression<Func<Order, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
        {
            "id" => o => o.OrderId,
            "orderdate" => o => o.OrderDate,
            "totalamount" => o => o.TotalAmount ?? 0,
            "status" => o => o.Status,
            "customerid" => o => o.CustomerId ?? 0,
            "userid" => o => o.UserId ?? 0,
            _ => o => o.OrderId
        };

        Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = q =>
        {
            var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
            return sortDesc ? ordered.ThenByDescending(o => o.OrderId) : ordered.ThenBy(o => o.OrderId);
        };

        var (items, totalCount) = await _orderRepository.GetPagedAsync(pageNumber, pageSize, filter, orderBy);
        var mappedItems = _mapper.Map<IEnumerable<OrderResponse>>(items);

        return (mappedItems, totalCount);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, int userId)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            UserId = userId,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0,
            DiscountAmount = 0
        };

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return _mapper.Map<OrderResponse>(order);
    }

    // ⭐ ĐÃ SỬA: Đảm bảo tải đầy đủ chi tiết và gọi RecalculateTotalAsync
    public async Task<OrderResponse?> UpdateAsync(int orderId, UpdateOrderRequest request)
    {
        // ⭐ THAY ĐỔI: Sử dụng GetByIdWithDetailsAsync để tải OrderItems
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId); 
        if (order == null) return null;

        // Chỉ update khi status = Pending
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot update order that is not pending");
        }

        order.CustomerId = request.CustomerId;
        
        // ⭐ THÊM: Buộc tính toán lại tổng tiền trước khi lưu (Quan trọng cho Bước 3)
        await RecalculateOrderTotalAsync(order);
        
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Không cần gọi GetByIdWithDetailsAsync lần nữa
        return _mapper.Map<OrderResponse>(order);
    }

    // Cancel Order (giữ nguyên)
    public async Task<bool> CancelAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        order.Status = OrderStatus.Canceled;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return true;
    }

    // ⭐ ĐÃ SỬA: Đảm bảo tải đầy đủ chi tiết
    public async Task<OrderResponse> AddItemAsync(int orderId, AddOrderItemRequest request)
    {
        // ⭐ THAY ĐỔI: Sử dụng GetByIdWithDetailsAsync để tải OrderItems
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId); 
        
        // Validate Order exists and is Pending (giữ nguyên)
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot add items to order that is not pending");
        }

        // Validate Product exists (giữ nguyên)
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        // Check Inventory availability (giữ nguyên)
        var inventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId);
        if (inventory == null)
        {
            throw new InvalidOperationException("Inventory not found for product");
        }
        if (inventory.Quantity < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient inventory. Available: {inventory.Quantity}, Requested: {request.Quantity}");
        }

        // ============================================================

        // Reserve inventory: Trừ trực tiếp Quantity (Direct deduction strategy)
        inventory.Quantity -= request.Quantity;
        await _inventoryRepository.UpdateAsync(inventory);

        // Check if item already exists in order
        var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == request.ProductId);
        if (existingItem != null)
        {
            // Update existing item
            existingItem.Quantity += request.Quantity;
            // ⭐ ĐÃ SỬA: Tính lại Subtotal từ Quantity * Price
            existingItem.Subtotal = existingItem.Quantity * existingItem.Price; 
        }
        else
        {
            // Add new item
            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = product.Price, // Snapshot price
                Subtotal = request.Quantity * product.Price
            };
            order.OrderItems.Add(orderItem);
        }

        // Update Order TotalAmount
        await RecalculateOrderTotalAsync(order); // Giữ nguyên, đã đúng

        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    // ⭐ ĐÃ SỬA: Đảm bảo tải đầy đủ chi tiết
    public async Task<OrderResponse> UpdateItemAsync(int orderId, int itemId, UpdateOrderItemRequest request)
    {
        // ⭐ THAY ĐỔI: Sử dụng GetByIdWithDetailsAsync để tải OrderItems
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId); 
        
        // Validate Order exists and is Pending (giữ nguyên)
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot update items in order that is not pending");
        }

        // Validate OrderItem exists
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == itemId);
        if (orderItem == null)
        {
            throw new InvalidOperationException("Order item not found");
        }

        // ============================================================

        // Calculate inventory delta (how much we need to adjust) (giữ nguyên logic)
        var quantityDelta = request.Quantity - orderItem.Quantity;

        // Handle inventory based on delta (giữ nguyên logic)
        if (quantityDelta > 0)
        {
             if (!orderItem.ProductId.HasValue)
            {
                throw new InvalidOperationException("Order item has no product ID");
            }

            var inventory = await _inventoryRepository.GetByProductIdAsync(orderItem.ProductId.Value);
            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory not found for product");
            }
            if (inventory.Quantity < quantityDelta)
            {
                throw new InvalidOperationException($"Insufficient inventory. Available: {inventory.Quantity}, Needed: {quantityDelta}");
            }

            // Reserve additional inventory
            inventory.Quantity -= quantityDelta;
            await _inventoryRepository.UpdateAsync(inventory);
        }
        else if (quantityDelta < 0)
        {
            // Release inventory back
            if (orderItem.ProductId.HasValue)
            {
                var inventory = await _inventoryRepository.GetByProductIdAsync(orderItem.ProductId.Value);
                if (inventory != null)
                {
                    inventory.Quantity += Math.Abs(quantityDelta);
                    await _inventoryRepository.UpdateAsync(inventory);
                }
            }
        }

        // Update OrderItem
        orderItem.Quantity = request.Quantity;
        // ⭐ ĐÃ SỬA: Tính lại Subtotal từ Quantity * Price
        orderItem.Subtotal = orderItem.Quantity * orderItem.Price; 

        // Update Order TotalAmount
        await RecalculateOrderTotalAsync(order); // Giữ nguyên, đã đúng

        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    // ⭐ ĐÃ SỬA: Đảm bảo tải đầy đủ chi tiết
    public async Task<OrderResponse> DeleteItemAsync(int orderId, int itemId)
    {
        // ⭐ THAY ĐỔI: Sử dụng GetByIdWithDetailsAsync để tải OrderItems
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId); 
        
        // Validate Order exists and is Pending (giữ nguyên)
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot delete items from order that is not pending");
        }

        // Validate OrderItem exists (giữ nguyên)
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == itemId);
        if (orderItem == null)
        {
            throw new InvalidOperationException("Order item not found");
        }

        // ============================================================

        // Release inventory back (giữ nguyên logic)
        if (orderItem.ProductId.HasValue)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(orderItem.ProductId.Value);
            if (inventory != null)
            {
                inventory.Quantity += orderItem.Quantity;
                await _inventoryRepository.UpdateAsync(inventory);
            }
        }

        // Remove OrderItem from collection
        order.OrderItems.Remove(orderItem);

        // Update Order TotalAmount
        await RecalculateOrderTotalAsync(order); // Giữ nguyên, đã đúng

        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }public async Task<OrderResponse> ApplyPromotionAsync(int orderId, ApplyPromotionRequest request)
{
    // 1️⃣ Lấy đơn hàng kèm chi tiết
    var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
    if (order == null)
        throw new InvalidOperationException("Order not found");

    if (order.Status != OrderStatus.Pending)
        throw new InvalidOperationException("Cannot apply promotion to order that is not pending");

    // 2️⃣ Lấy thông tin khuyến mãi
    var promotion = await _promotionRepository.GetByPromoCodeAsync(request.PromoCode);
    if (promotion == null)
        throw new InvalidOperationException("Promotion not found");

    if (!string.Equals(promotion.Status, "active", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Promotion is not active");

    var now = DateTime.UtcNow;
    if (now < promotion.StartDate)
        throw new InvalidOperationException("Promotion has not started yet");
    if (now > promotion.EndDate)
        throw new InvalidOperationException("Promotion has expired");

    // 3️⃣ Bảo đảm có OrderItems
    if (order.OrderItems == null || !order.OrderItems.Any())
        throw new InvalidOperationException("Order has no items");

    // 4️⃣ Tính lại tổng tiền từ sản phẩm
    order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

    // 5️⃣ Kiểm tra tổng tiền tối thiểu
    if (promotion.MinOrderAmount > 0 && order.TotalAmount < promotion.MinOrderAmount)
        throw new InvalidOperationException($"Order amount must be at least {promotion.MinOrderAmount:C}");

    // 6️⃣ Kiểm tra giới hạn sử dụng
    if (promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
        throw new InvalidOperationException("Promotion usage limit has been reached");

    // 7️⃣ Áp dụng mã khuyến mãi
    order.PromoId = promotion.PromoId;
    order.DiscountAmount = await CalculateDiscountAsync(promotion, order.TotalAmount ?? 0);
    
    // 8️⃣ Lưu thay đổi
    await _orderRepository.UpdateAsync(order);
    await _orderRepository.SaveChangesAsync();

    // 9️⃣ Trả kết quả
    var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
    return _mapper.Map<OrderResponse>(updatedOrder!);
}

    public async Task<OrderResponse> RemovePromotionAsync(int orderId)
    {
        // ⭐ THAY ĐỔI: Sử dụng GetByIdWithDetailsAsync để tải OrderItems
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId); 
        
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot remove promotion from order that is not pending");
        }

        if (!order.PromoId.HasValue)
        {
            throw new InvalidOperationException("No promotion applied to this order");
        }

        // Remove promotion
        order.PromoId = null;
        order.DiscountAmount = 0;

        // ⭐ THÊM: Tính toán lại tổng tiền sau khi xóa khuyến mãi
        await RecalculateOrderTotalAsync(order);
        
        await _orderRepository.SaveChangesAsync();

        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    // CheckoutAsync
public async Task<OrderResponse> CheckoutAsync(int orderId, CheckoutRequest request)
{
    var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
    if (order == null)
        throw new InvalidOperationException("Order not found");

    if (order.Status != OrderStatus.Pending)
        throw new InvalidOperationException("Cannot checkout order that is not pending");

    if (order.OrderItems.Count == 0)
        throw new InvalidOperationException("Cannot checkout empty order");

    await RecalculateOrderTotalAsync(order);
        await _orderRepository.UpdateAsync(order);   // ✅ cập nhật lại giá trị tính toán mới
        await _orderRepository.SaveChangesAsync(); 
    // kiểm tra tồn kho
    foreach (var item in order.OrderItems)
    {
        if (item.ProductId.HasValue)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId.Value);
            if (inventory == null)
                throw new InvalidOperationException($"Inventory not found for product ID: {item.ProductId.Value}");
            if (inventory.Quantity < 0)
                throw new InvalidOperationException($"Insufficient inventory for product ID: {item.ProductId.Value}");
        }
    }

    var finalAmount = order.TotalAmount - order.DiscountAmount;
    if (finalAmount < 0) finalAmount = 0;

    if (request.Amount != finalAmount)
        throw new InvalidOperationException($"Payment amount {request.Amount:C} does not match order amount {finalAmount:C}");

    if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        throw new InvalidOperationException($"Invalid payment method: {request.PaymentMethod}");

    // ✅ Lưu khách hàng
    if (request.CustomerId.HasValue)
        order.CustomerId = request.CustomerId;

    var payment = new Payment
    {
        OrderId = orderId,
        Amount = request.Amount,
        PaymentMethod = paymentMethod,
        PaymentDate = DateTime.UtcNow
    };
    await _paymentRepository.AddAsync(payment);

    order.Status = OrderStatus.Paid;

    if (order.PromoId.HasValue)
    {
        var promotion = await _promotionRepository.GetByIdAsync(order.PromoId.Value);
        if (promotion != null)
        {
            promotion.UsedCount++;
            await _promotionRepository.UpdateAsync(promotion);
        }
    }

    await _orderRepository.SaveChangesAsync();

    var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
    return _mapper.Map<OrderResponse>(updatedOrder!);
}


}
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
        var totalAmount = order.OrderItems.Sum(oi => oi.Subtotal);
        order.TotalAmount = totalAmount;

        // Recalculate discount if promotion exists
        if (order.PromoId.HasValue)
        {
            var promotion = await _promotionRepository.GetByIdAsync(order.PromoId.Value);
            if (promotion != null && order.TotalAmount.HasValue)
            {
                order.DiscountAmount = await CalculateDiscountAsync(promotion, order.TotalAmount.Value);
            }
        }
        else
        {
            order.DiscountAmount = 0;
        }
    }

    // Helper method to calculate discount
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
        int pageNumber, int pageSize, OrderStatus? status = null, int? userId = null, int? customerId = null)
    {
        System.Linq.Expressions.Expression<Func<Order, bool>>? filter = null;

        if (status.HasValue || userId.HasValue || customerId.HasValue)
        {
            filter = o =>
                (!status.HasValue || o.Status == status.Value) &&
                (!userId.HasValue || o.UserId == userId) &&
                (!customerId.HasValue || o.CustomerId == customerId);
        }

        var (items, totalCount) = await _orderRepository.GetPagedAsync(pageNumber, pageSize, filter);
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

    public async Task<OrderResponse?> UpdateAsync(int orderId, UpdateOrderRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        // Chỉ update khi status = Pending
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot update order that is not pending");
        }

        order.CustomerId = request.CustomerId;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return updatedOrder != null ? _mapper.Map<OrderResponse>(updatedOrder) : null;
    }

    // Cancel Order
    public async Task<bool> CancelAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        order.Status = OrderStatus.Canceled;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return true;
    }

    // Order Items Management - WITH INVENTORY RESERVE
    // ============================================================
    // TRANSACTION #1: AddOrderItem
    // - Reserve inventory: Directly deduct Inventory.Quantity
    // - Add/Update OrderItem in cart
    // - Update Order.TotalAmount
    // - All operations wrapped in EF Core Unit of Work transaction
    // - Rollback if: Inventory insufficient, order not pending, or any update fails
    // ============================================================
    public async Task<OrderResponse> AddItemAsync(int orderId, AddOrderItemRequest request)
    {
        // Validate Order exists and is Pending
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot add items to order that is not pending");
        }

        // Validate Product exists
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        // Check Inventory availability
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
        // TRANSACTION START: Reserve inventory and add item
        // Uses EF Core implicit transaction via SaveChangesAsync()
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
        await RecalculateOrderTotalAsync(order);

        // ============================================================
        // TRANSACTION COMMIT: Save all changes atomically
        // Rollback if: Any update fails
        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    // ============================================================
    // TRANSACTION #2: UpdateOrderItem
    // - Calculate inventory delta (new qty - old qty)
    // - Reserve additional inventory if qty increased
    // - Release inventory if qty decreased
    // - Update OrderItem quantity & subtotal
    // - Update Order.TotalAmount
    // - All operations wrapped in EF Core Unit of Work transaction
    // - Rollback if: Insufficient inventory, concurrent update, or any failure
    // ============================================================
    public async Task<OrderResponse> UpdateItemAsync(int orderId, int itemId, UpdateOrderItemRequest request)
    {
        // Validate Order exists and is Pending
        var order = await _orderRepository.GetByIdAsync(orderId);
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
        // TRANSACTION START: Update item with inventory delta
        // ============================================================

        // Calculate inventory delta (how much we need to adjust)
        var quantityDelta = request.Quantity - orderItem.Quantity;

        // Handle inventory based on delta
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
        orderItem.Subtotal = orderItem.Quantity * orderItem.Price;

        // Update Order TotalAmount
        await RecalculateOrderTotalAsync(order);

        // ============================================================
        // TRANSACTION COMMIT: Save all changes atomically
        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    // ============================================================
    // TRANSACTION #3: DeleteOrderItem
    // - Release inventory: Hoàn lại Inventory.Quantity
    // - Remove OrderItem from order
    // - Update Order.TotalAmount
    // - All operations wrapped in EF Core Unit of Work transaction
    // - Rollback if: Any update fails
    // ============================================================
    public async Task<OrderResponse> DeleteItemAsync(int orderId, int itemId)
    {
        // Validate Order exists and is Pending
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot delete items from order that is not pending");
        }

        // Validate OrderItem exists
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == itemId);
        if (orderItem == null)
        {
            throw new InvalidOperationException("Order item not found");
        }

        // ============================================================
        // TRANSACTION START: Release inventory and delete item
        // ============================================================

        // Release inventory back
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
        await RecalculateOrderTotalAsync(order);

        // ============================================================
        // TRANSACTION COMMIT: Save all changes atomically
        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    // ============================================================
    // TRANSACTION #4: ApplyPromotion
    // - Validate promotion rules (active, date range, usage limit, min amount)
    // - Calculate DiscountAmount (Percent or Fixed)
    // - Update Order.PromoId and DiscountAmount
    // - Recalculate Order.TotalAmount with discount
    // - All operations wrapped in EF Core Unit of Work transaction
    // - Rollback if: Promotion invalid, concurrent usage, or any failure
    // ============================================================
    // Promotion
    public async Task<OrderResponse> ApplyPromotionAsync(int orderId, ApplyPromotionRequest request)
    {
        // Validate Order exists and is Pending
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot apply promotion to order that is not pending");
        }

        // Get Promotion by code
        var promotion = await _promotionRepository.GetByPromoCodeAsync(request.PromoCode);
        if (promotion == null)
        {
            throw new InvalidOperationException("Promotion not found");
        }

        // Validate Promotion is active
        if (promotion.Status.ToLower() != "active")
        {
            throw new InvalidOperationException("Promotion is not active");
        }

        // Validate date range
        var now = DateTime.Now;
        if (now < promotion.StartDate)
        {
            throw new InvalidOperationException("Promotion has not started yet");
        }
        if (now > promotion.EndDate)
        {
            throw new InvalidOperationException("Promotion has expired");
        }

        // Validate minimum order amount
        var totalAmount = order.OrderItems.Sum(oi => oi.Subtotal);
        if (totalAmount < promotion.MinOrderAmount)
        {
            throw new InvalidOperationException($"Order amount must be at least {promotion.MinOrderAmount:C}");
        }

        // Validate usage limit
        if (promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
        {
            throw new InvalidOperationException("Promotion usage limit has been reached");
        }

        // Apply promotion to order
        order.PromoId = promotion.PromoId;
        await RecalculateOrderTotalAsync(order);

        // ============================================================
        // TRANSACTION COMMIT: Save promotion application
        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    public async Task<OrderResponse> RemovePromotionAsync(int orderId)
    {
        // Validate Order exists and is Pending
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot remove promotion from order that is not pending");
        }

        // Check if promotion exists
        if (!order.PromoId.HasValue)
        {
            throw new InvalidOperationException("No promotion applied to this order");
        }

        // Remove promotion
        order.PromoId = null;
        order.DiscountAmount = 0;

        // Save changes
        await _orderRepository.SaveChangesAsync();

        // Return updated order
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }

    // ============================================================
    // TRANSACTION #5: Checkout (Core Payment Transaction)
    // - Validate order is pending & has items
    // - Double-check inventory for all items
    // - Validate payment amount matches order total (after discount)
    // - Create Payment record
    // - Update Order.Status = Paid
    // - Increment Promotion.UsedCount (if applicable)
    // - Commit inventory changes (final deduction already done in AddItem)
    // - All operations wrapped in EF Core Unit of Work transaction
    // - Rollback if: Inventory insufficient, payment amount mismatch, or any failure
    // ============================================================
    public async Task<OrderResponse> CheckoutAsync(int orderId, CheckoutRequest request)
    {
        // 1. Validate Order exists and is Pending
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot checkout order that is not pending");
        }

        // 2. Validate Order has items
        if (order.OrderItems.Count == 0)
        {
            throw new InvalidOperationException("Cannot checkout empty order");
        }

        // 3. Double-check inventory for all items
        // Note: Inventory is already deducted during AddItem (Direct deduction strategy)
        // This is just a final verification before committing payment
        foreach (var item in order.OrderItems)
        {
            if (item.ProductId.HasValue)
            {
                var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId.Value);
                if (inventory == null)
                {
                    throw new InvalidOperationException($"Inventory not found for product ID: {item.ProductId.Value}");
                }
                // Verify inventory is not negative (prevent overselling)
                if (inventory.Quantity < 0)
                {
                    throw new InvalidOperationException($"Insufficient inventory for product ID: {item.ProductId.Value}");
                }
            }
        }

        // 4. Validate payment amount (must be equal to final amount after discount)
        var finalAmount = order.TotalAmount - order.DiscountAmount;
        if (request.Amount != finalAmount)
        {
            throw new InvalidOperationException($"Payment amount {request.Amount:C} does not match order amount {finalAmount:C}");
        }

        // 5. Parse PaymentMethod enum
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        {
            throw new InvalidOperationException($"Invalid payment method: {request.PaymentMethod}");
        }

        // ============================================================
        // TRANSACTION START: Process payment and complete order
        // ============================================================

        // 6. Create Payment record
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = request.Amount,
            PaymentMethod = paymentMethod,
            PaymentDate = DateTime.UtcNow
        };
        await _paymentRepository.AddAsync(payment);

        // 7. Update Order status to Paid
        order.Status = OrderStatus.Paid;

        // 8. Increment Promotion.UsedCount if applicable
        if (order.PromoId.HasValue)
        {
            var promotion = await _promotionRepository.GetByIdAsync(order.PromoId.Value);
            if (promotion != null)
            {
                promotion.UsedCount++;
                await _promotionRepository.UpdateAsync(promotion);
            }
        }
        // 9. Save all changes (Transaction handled by EF Core Unit of Work)
        // ============================================================
        // TRANSACTION COMMIT: Save all changes atomically
        // - Commit payment record
        // - Commit order status change
        // - Commit promotion usage increment
        // - Rollback if: Any operation fails
        // ============================================================
        await _orderRepository.SaveChangesAsync();

        // 10. Return updated order with payment details
        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        return _mapper.Map<OrderResponse>(updatedOrder!);
    }
}

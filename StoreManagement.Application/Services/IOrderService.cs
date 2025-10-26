using StoreManagement.Application.DTOs.Order;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public interface IOrderService
{
    // Order CRUD
    Task<OrderResponse?> GetByIdAsync(int orderId);
    Task<(IEnumerable<OrderResponse> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, int pageSize, OrderStatus? status = null, int? userId = null, int? customerId = null);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, int userId);
    Task<OrderResponse?> UpdateAsync(int orderId, UpdateOrderRequest request);
    Task<bool> CancelAsync(int orderId);

    // Order Items Management
    Task<OrderResponse> AddItemAsync(int orderId, AddOrderItemRequest request);
    Task<OrderResponse> UpdateItemAsync(int orderId, int itemId, UpdateOrderItemRequest request);
    Task<OrderResponse> DeleteItemAsync(int orderId, int itemId);

    // Promotion
    Task<OrderResponse> ApplyPromotionAsync(int orderId, ApplyPromotionRequest request);
    Task<OrderResponse> RemovePromotionAsync(int orderId);

    // Checkout & Payment
    Task<OrderResponse> CheckoutAsync(int orderId, CheckoutRequest request);
}

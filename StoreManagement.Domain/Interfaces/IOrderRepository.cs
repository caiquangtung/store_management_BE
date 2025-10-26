using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<Order>> GetByUserAsync(int userId);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<bool> OrderNumberExistsAsync(string orderNumber);

    // NEW: Method để load Order với OrderItems và navigation properties
    Task<Order?> GetByIdWithDetailsAsync(int orderId);
}

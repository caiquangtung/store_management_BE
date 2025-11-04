using StoreManagement.Domain.Entities;
namespace StoreManagement.Domain.Interfaces;
public class SalesSummaryRawData
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int NumberOfOrders { get; set; }
}
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<Order>> GetByUserAsync(int userId);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<bool> OrderNumberExistsAsync(string orderNumber);

    // NEW: Method để load Order với OrderItems và navigation properties
    Task<Order?> GetByIdWithDetailsAsync(int orderId);
    Task<IEnumerable<SalesSummaryRawData>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate, string groupBy);
    Task<IEnumerable<OrderItem>> GetLedgerMovementsAsync(int productId, DateTime? startDate, DateTime? endDate);
}

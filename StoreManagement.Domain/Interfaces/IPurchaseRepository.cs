using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;
public class PurchaseSummaryRawData
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int NumberOfPurchases { get; set; }
}
public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<Purchase?> GetByIdWithDetailsAsync(int purchaseId);
    Task<IEnumerable<PurchaseItem>> GetLedgerMovementsAsync(int productId, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<PurchaseSummaryRawData>> GetPurchaseSummaryAsync(DateTime startDate, DateTime endDate, string groupBy);
}
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;

namespace StoreManagement.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId);
    Task<Product?> GetBySKUAsync(string sku);
    Task<bool> SKUExistsAsync(string sku);
    Task<IEnumerable<ABCData>> GetABCAnalysisDataAsync(DateTime? fromDate = null, DateTime? toDate = null);  // Updated with date filters
    Task<IEnumerable<Product>> GetDeadStockProductsAsync(DateTime startDate, DateTime endDate);
}

public class ABCData  // Helper class for raw ABC data
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal? Value { get; set; }  // SUM(price * quantity)
    public int Frequency { get; set; }  // COUNT(DISTINCT order_id)
    public decimal Score { get; set; }  // Value * Frequency (nullable-safe)
}
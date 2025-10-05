using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId);
    Task<Product?> GetBySKUAsync(string sku);
    Task<bool> SKUExistsAsync(string sku);
}

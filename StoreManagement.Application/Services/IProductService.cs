using StoreManagement.Application.DTOs.Products;

namespace StoreManagement.Application.Services;

public interface IProductService
{
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<ProductResponse?> CreateAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> SKUExistsAsync(string sku);
}
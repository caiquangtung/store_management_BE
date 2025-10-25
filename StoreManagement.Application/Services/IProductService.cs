using StoreManagement.Application.DTOs.Products;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.Services;

public interface IProductService
{
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize,EntityStatus? status = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false);
    Task<ProductResponse?> CreateAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> SKUExistsAsync(string sku);
    Task<IEnumerable<ABCProductResponse>> GetABCAnalysisAsync(DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10);  // Updated with date filters and pagination
}
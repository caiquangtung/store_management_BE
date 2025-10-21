using StoreManagement.Application.DTOs.Suppliers;

namespace StoreManagement.Application.Services;

public interface ISupplierService
{
    Task<SupplierResponse?> GetByIdAsync(int id);
    Task<IEnumerable<SupplierResponse>> GetAllAsync();
    Task<(IEnumerable<SupplierResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDesc = false);
    Task<SupplierResponse?> CreateAsync(CreateSupplierRequest request);
    Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(int id);
}
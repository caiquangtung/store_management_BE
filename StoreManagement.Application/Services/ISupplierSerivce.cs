using StoreManagement.Application.DTOs.Suppliers;

namespace StoreManagement.Application.Services;

public interface ISupplierService
{
    Task<SupplierResponse?> GetByIdAsync(int id);
    Task<IEnumerable<SupplierResponse>> GetAllAsync();
    Task<SupplierResponse?> CreateAsync(CreateSupplierRequest request);
    Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(int id);
}
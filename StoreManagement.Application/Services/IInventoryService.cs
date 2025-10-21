using StoreManagement.Application.DTOs.Inventory;

namespace StoreManagement.Application.Services;

public interface IInventoryService
{
    Task<InventoryResponse?> GetByIdAsync(int id);
    Task<IEnumerable<InventoryResponse>> GetAllAsync();
    Task<(IEnumerable<InventoryResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, int? productId = null, string? sortBy = null, bool sortDesc = false);
    Task<List<InventoryResponse>> CreateAsync(CreateInventoryRequest request);
    Task<InventoryResponse?> UpdateAsync(int id, UpdateInventoryRequest request);
    Task<bool> SetQuantityToZeroAsync(int id);  // Replace DELETE
    Task<IEnumerable<LowStockResponse>> GetLowStockAsync(int threshold = 10);
}
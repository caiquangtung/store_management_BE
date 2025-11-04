using StoreManagement.Application.DTOs.InventoryAdjustment;

namespace StoreManagement.Application.Services;

public interface IInventoryAdjustmentService
{
    Task<AdjustmentResponse> CreateAdjustmentAsync(CreateAdjustmentRequest request, int userId);
    Task<(IEnumerable<AdjustmentResponse> Items, int TotalCount)> GetAllAdjustmentsPagedAsync(int pageNumber, int pageSize, int? productId = null, string? sortBy = null, bool sortDesc = false);
}
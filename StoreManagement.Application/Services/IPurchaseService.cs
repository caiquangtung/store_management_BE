using StoreManagement.Application.DTOs.Purchase;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request, int userId);
    Task<PurchaseResponse?> GetPurchaseByIdAsync(int purchaseId);
    Task<(IEnumerable<PurchaseResponse> Items, int TotalCount)> GetAllPurchasesPagedAsync(int pageNumber, int pageSize, PurchaseStatus? status = null, int? supplierId = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false);
    Task<PurchaseResponse> ConfirmPurchaseAsync(int purchaseId);
    Task<PurchaseResponse> CancelPurchaseAsync(int purchaseId);
}
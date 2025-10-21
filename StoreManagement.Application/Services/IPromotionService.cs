using StoreManagement.Application.Common;
using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.DTOs.Promotion;

namespace StoreManagement.Application.Services;

public interface IPromotionService
{
    Task<IEnumerable<PromotionResponse>> GetPromotionsAsync(string? searchTerm = null);
    Task<(IEnumerable<PromotionResponse> Items, int TotalCount)> GetPromotionsPagedAsync(
        int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDesc = false);
    Task<PromotionResponse?> GetPromotionByIdAsync(int promotionId);
    Task<PromotionResponse?> GetPromotionByCodeAsync(string promoCode);
    Task<IEnumerable<PromotionResponse>> GetActivePromotionsAsync();
    Task<bool> PromoCodeExistsAsync(string promoCode);
    Task<bool> PromoCodeExistsForOtherPromotionAsync(string promoCode, int promotionId);
    Task<PromotionResponse> CreatePromotionAsync(CreatePromotionRequest request);
    Task<PromotionResponse?> UpdatePromotionAsync(int id, UpdatePromotionRequest request);
    Task<bool> DeletePromotionAsync(int promotionId);
    Task<PromotionValidationResponse> ValidatePromotionAsync(ValidatePromotionRequest request);
    Task<decimal> CalculateDiscountAsync(int promotionId, decimal orderAmount);
    Task<decimal> CalculateDiscountAsync(PromotionResponse promotion, decimal orderAmount);
    Task IncrementUsageCountAsync(int promotionId);
    Task DeactivateExpiredPromotionsAsync();
    Task<bool> IsPromotionValidAsync(int promotionId, decimal orderAmount);
}

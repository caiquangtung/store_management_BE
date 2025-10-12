using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface IPromotionRepository : IRepository<Promotion>
{
    Task<Promotion?> GetByPromoCodeAsync(string promoCode);
    Task<bool> PromoCodeExistsAsync(string promoCode);
    Task<bool> PromoCodeExistsForOtherPromotionAsync(string promoCode, int promotionId);
    Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
    Task<IEnumerable<Promotion>> GetExpiredPromotionsAsync();
    Task IncrementUsageCountAsync(int promotionId);
    Task DeactivateExpiredPromotionsAsync();
}

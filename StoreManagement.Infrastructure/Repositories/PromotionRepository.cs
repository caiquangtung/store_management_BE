using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.Repositories;

public class PromotionRepository : BaseRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(StoreDbContext context) : base(context)
    {
    }

    public async Task<Promotion?> GetByPromoCodeAsync(string promoCode)
    {
        return await _context.Promotions
            .FirstOrDefaultAsync(p => p.PromoCode == promoCode);
    }

    public async Task<bool> PromoCodeExistsAsync(string promoCode)
    {
        return await _context.Promotions
            .AnyAsync(p => p.PromoCode == promoCode);
    }

    public async Task<bool> PromoCodeExistsForOtherPromotionAsync(string promoCode, int promotionId)
    {
        return await _context.Promotions
            .AnyAsync(p => p.PromoCode == promoCode && p.PromoId != promotionId);
    }

    public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
    {
        var now = DateTime.Now;
        return await _context.Promotions
            .Where(p => p.Status == "active" &&
                       p.StartDate <= now &&
                       p.EndDate >= now)
            .ToListAsync();
    }

    public async Task<IEnumerable<Promotion>> GetExpiredPromotionsAsync()
    {
        var now = DateTime.Now;
        return await _context.Promotions
            .Where(p => p.EndDate < now && p.Status == "active")
            .ToListAsync();
    }

    public async Task IncrementUsageCountAsync(int promotionId)
    {
        var promotion = await _context.Promotions.FindAsync(promotionId);
        if (promotion != null)
        {
            promotion.UsedCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeactivateExpiredPromotionsAsync()
    {
        var expiredPromotions = await GetExpiredPromotionsAsync();
        foreach (var promotion in expiredPromotions)
        {
            promotion.Status = "inactive";
        }
        await _context.SaveChangesAsync();
    }
}

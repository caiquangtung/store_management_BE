using AutoMapper;
using StoreManagement.Application.Common;
using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.DTOs.Promotion;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;
using StoreManagement.Domain.Interfaces;

namespace StoreManagement.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IMapper _mapper;

    public PromotionService(IPromotionRepository promotionRepository, IMapper mapper)
    {
        _promotionRepository = promotionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PromotionResponse>> GetPromotionsAsync(string? searchTerm = null)
    {
        IEnumerable<Promotion> promotions;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            promotions = await _promotionRepository.FindAsync(p =>
                p.PromoCode.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm)));
        }
        else
        {
            promotions = await _promotionRepository.GetAllAsync();
        }

        return _mapper.Map<IEnumerable<PromotionResponse>>(promotions.OrderByDescending(p => p.StartDate));
    }

    public async Task<PromotionResponse?> GetPromotionByIdAsync(int promotionId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        return promotion != null ? _mapper.Map<PromotionResponse>(promotion) : null;
    }

    public async Task<PromotionResponse?> GetPromotionByCodeAsync(string promoCode)
    {
        var promotion = await _promotionRepository.GetByPromoCodeAsync(promoCode);
        return promotion != null ? _mapper.Map<PromotionResponse>(promotion) : null;
    }

    public async Task<IEnumerable<PromotionResponse>> GetActivePromotionsAsync()
    {
        var activePromotions = await _promotionRepository.GetActivePromotionsAsync();
        return _mapper.Map<IEnumerable<PromotionResponse>>(activePromotions);
    }

    public async Task<bool> PromoCodeExistsAsync(string promoCode)
    {
        return await _promotionRepository.PromoCodeExistsAsync(promoCode);
    }

    public async Task<bool> PromoCodeExistsForOtherPromotionAsync(string promoCode, int promotionId)
    {
        return await _promotionRepository.PromoCodeExistsForOtherPromotionAsync(promoCode, promotionId);
    }

    public async Task<PromotionResponse> CreatePromotionAsync(CreatePromotionRequest request)
    {
        var promotion = _mapper.Map<Promotion>(request);
        var createdPromotion = await _promotionRepository.AddAsync(promotion);
        await _promotionRepository.SaveChangesAsync();
        return _mapper.Map<PromotionResponse>(createdPromotion);
    }

    public async Task<PromotionResponse?> UpdatePromotionAsync(UpdatePromotionRequest request)
    {
        var existingPromotion = await _promotionRepository.GetByIdAsync(request.PromoId);
        if (existingPromotion == null)
            return null;

        _mapper.Map(request, existingPromotion);
        var updatedPromotion = await _promotionRepository.UpdateAsync(existingPromotion);
        await _promotionRepository.SaveChangesAsync();
        return _mapper.Map<PromotionResponse>(updatedPromotion);
    }

    public async Task<bool> DeletePromotionAsync(int promotionId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        if (promotion == null)
            return false;

        await _promotionRepository.DeleteAsync(promotion);
        await _promotionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<PromotionValidationResponse> ValidatePromotionAsync(ValidatePromotionRequest request)
    {
        var response = new PromotionValidationResponse
        {
            IsValid = false,
            Message = "Promotion not found",
            DiscountAmount = 0,
            FinalAmount = request.OrderAmount
        };

        // Get promotion by code
        var promotion = await _promotionRepository.GetByPromoCodeAsync(request.PromoCode);
        if (promotion == null)
            return response;

        response.Promotion = _mapper.Map<PromotionResponse>(promotion);

        // Validate promotion
        var validationResult = await ValidatePromotionRulesAsync(promotion, request.OrderAmount);
        if (!validationResult.IsValid)
        {
            response.Message = validationResult.Message;
            return response;
        }

        // Calculate discount
        var discountAmount = await CalculateDiscountAsync(promotion, request.OrderAmount);
        var finalAmount = request.OrderAmount - discountAmount;

        response.IsValid = true;
        response.Message = "Promotion is valid";
        response.DiscountAmount = discountAmount;
        response.FinalAmount = Math.Max(0, finalAmount); // Ensure final amount is not negative

        return response;
    }

    public async Task<decimal> CalculateDiscountAsync(int promotionId, decimal orderAmount)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        if (promotion == null)
            return 0;

        return await CalculateDiscountAsync(promotion, orderAmount);
    }

    public async Task<decimal> CalculateDiscountAsync(PromotionResponse promotion, decimal orderAmount)
    {
        // Create a temporary Promotion entity for calculation
        var tempPromotion = new Promotion
        {
            DiscountType = promotion.DiscountType,
            DiscountValue = promotion.DiscountValue
        };

        return await CalculateDiscountAsync(tempPromotion, orderAmount);
    }

    private Task<decimal> CalculateDiscountAsync(Promotion promotion, decimal orderAmount)
    {
        decimal discount;
        if (promotion.DiscountType == DiscountType.Percent)
        {
            discount = orderAmount * (promotion.DiscountValue / 100);
        }
        else // Fixed
        {
            discount = Math.Min(promotion.DiscountValue, orderAmount);
        }
        return Task.FromResult(discount);
    }

    public Task IncrementUsageCountAsync(int promotionId)
    {
        return _promotionRepository.IncrementUsageCountAsync(promotionId);
    }

    public Task DeactivateExpiredPromotionsAsync()
    {
        return _promotionRepository.DeactivateExpiredPromotionsAsync();
    }

    public async Task<bool> IsPromotionValidAsync(int promotionId, decimal orderAmount)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        if (promotion == null)
            return false;

        var validationResult = await ValidatePromotionRulesAsync(promotion, orderAmount);
        return validationResult.IsValid;
    }

    private Task<(bool IsValid, string Message)> ValidatePromotionRulesAsync(Promotion promotion, decimal orderAmount)
    {
        var now = DateTime.Now;

        // Check if promotion is active
        if (promotion.Status.ToLower() != "active")
        {
            return Task.FromResult((false, "Promotion is not active"));
        }

        // Check if promotion is within valid date range
        if (now < promotion.StartDate)
        {
            return Task.FromResult((false, "Promotion has not started yet"));
        }

        if (now > promotion.EndDate)
        {
            return Task.FromResult((false, "Promotion has expired"));
        }

        // Check if order amount meets minimum requirement
        if (orderAmount < promotion.MinOrderAmount)
        {
            return Task.FromResult((false, $"Order amount must be at least {promotion.MinOrderAmount:C}"));
        }

        // Check if usage limit is reached
        if (promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
        {
            return Task.FromResult((false, "Promotion usage limit has been reached"));
        }

        return Task.FromResult((true, "Promotion is valid"));
    }
}

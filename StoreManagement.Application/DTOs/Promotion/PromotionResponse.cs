using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.DTOs.Promotion;

public class PromotionResponse
{
    public int PromoId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsUsageLimitReached { get; set; }
}

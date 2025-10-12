namespace StoreManagement.Application.DTOs.Promotion;

public class ValidatePromotionRequest
{
    public string PromoCode { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

namespace StoreManagement.Application.DTOs.Promotion;

public class PromotionValidationResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public PromotionResponse? Promotion { get; set; }
}

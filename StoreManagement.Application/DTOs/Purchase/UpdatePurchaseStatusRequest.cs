namespace StoreManagement.Application.DTOs.Purchase;

// Dùng cho cả confirm và cancel
public class UpdatePurchaseStatusRequest
{
    public string Status { get; set; } = string.Empty; // "Confirmed" hoặc "Canceled"
}
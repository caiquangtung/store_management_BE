namespace StoreManagement.Application.DTOs.Order;

public class CheckoutRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal CustomerPaid { get; set; }
    public string? PromoCode { get; set; }

    // ✅ Thêm 2 dòng này
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
}

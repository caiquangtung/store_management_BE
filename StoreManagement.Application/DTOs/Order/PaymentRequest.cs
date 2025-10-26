namespace StoreManagement.Application.DTOs.Order;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
}

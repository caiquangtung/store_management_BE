namespace StoreManagement.Application.DTOs.Order;

public class CheckoutRequest
{
    public string PaymentMethod { get; set; } = "Cash";
    public decimal Amount { get; set; }
}

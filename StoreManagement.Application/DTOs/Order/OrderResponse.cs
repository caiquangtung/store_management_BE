namespace StoreManagement.Application.DTOs.Order;

public class OrderResponse
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? PromoId { get; set; }
     public string? PromoCode { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
    public List<PaymentResponse> Payments { get; set; } = new();
}

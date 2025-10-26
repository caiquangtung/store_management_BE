namespace StoreManagement.Application.DTOs.Order;

public class OrderItemResponse
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}

namespace StoreManagement.Application.DTOs.Purchase;

public class PurchaseItemResponse
{
    public int PurchaseItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Subtotal { get; set; }
}
namespace StoreManagement.Application.DTOs.Purchase;

public class PurchaseItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
}
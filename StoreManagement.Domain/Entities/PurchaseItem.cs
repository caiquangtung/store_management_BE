namespace StoreManagement.Domain.Entities;

public class PurchaseItem
{
    public int PurchaseItemId { get; set; }
    public int PurchaseId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Subtotal { get; set; } // Sẽ được tính bằng generated column trong DB

    public virtual Purchase Purchase { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
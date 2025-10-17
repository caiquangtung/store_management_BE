namespace StoreManagement.Application.DTOs.Inventory;

public class LowStockResponse
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int Threshold { get; set; }
    public int ReorderQuantity { get; set; }  // Suggestion: threshold - quantity
    public bool IsLowStock { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
    public ProductInfo Product { get; set; } = new();
}
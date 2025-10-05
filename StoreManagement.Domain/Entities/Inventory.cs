namespace StoreManagement.Domain.Entities;

public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 0;
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}

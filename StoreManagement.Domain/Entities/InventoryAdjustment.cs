namespace StoreManagement.Domain.Entities;

public class InventoryAdjustment
{
    public int AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public int? UserId { get; set; }
    public int Quantity { get; set; } // Âm hoặc dương
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual User? User { get; set; }
}
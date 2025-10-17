using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Domain.Entities;

public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Product Product { get; set; } = null!;
}
namespace StoreManagement.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public string Unit { get; set; } = "pcs";
    public DateTime CreatedAt { get; set; }
    public string? ImagePath { get; set; } = null;  

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual Supplier? Supplier { get; set; }
    public virtual ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

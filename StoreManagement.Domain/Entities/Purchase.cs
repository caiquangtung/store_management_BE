using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class Purchase
{
    public int PurchaseId { get; set; }
    public int? SupplierId { get; set; }
    public int? UserId { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Supplier? Supplier { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
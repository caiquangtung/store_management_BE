namespace StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;
public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

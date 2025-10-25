namespace StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;
public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

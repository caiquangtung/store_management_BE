namespace StoreManagement.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

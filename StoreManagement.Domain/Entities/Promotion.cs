using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class Promotion
{
    public int PromoId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public int UsageLimit { get; set; } = 0;
    public int UsedCount { get; set; } = 0;
    public string Status { get; set; } = "active";

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

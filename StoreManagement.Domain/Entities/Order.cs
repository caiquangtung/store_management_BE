using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public int? UserId { get; set; }
    public int? PromoId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal? TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;

    // Navigation properties
    public virtual Customer? Customer { get; set; }
    public virtual User? User { get; set; }
    public virtual Promotion? Promotion { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

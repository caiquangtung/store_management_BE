using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public DateTime PaymentDate { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}

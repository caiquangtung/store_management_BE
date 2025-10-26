using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Staff;
    public DateTime CreatedAt { get; set; }

    public EntityStatus Status { get; set; } = EntityStatus.Active;
    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

namespace StoreManagement.Application.DTOs.Customer;
using StoreManagement.Domain.Enums;
public class CustomerResponse
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public EntityStatus Status { get; set; }
}

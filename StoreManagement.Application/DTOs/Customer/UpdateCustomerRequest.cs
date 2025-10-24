namespace StoreManagement.Application.DTOs.Customer;
using StoreManagement.Domain.Enums;
public class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public EntityStatus? Status { get; set; }
}

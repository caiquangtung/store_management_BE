namespace StoreManagement.Application.DTOs.Suppliers;
using StoreManagement.Domain.Enums;
public class UpdateSupplierRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public EntityStatus? Status { get; set; }
}
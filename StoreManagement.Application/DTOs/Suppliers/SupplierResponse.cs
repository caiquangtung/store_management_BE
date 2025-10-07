namespace StoreManagement.Application.DTOs.Suppliers;

public class SupplierResponse
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
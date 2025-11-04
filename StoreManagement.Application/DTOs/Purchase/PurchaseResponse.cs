using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.DTOs.Purchase;

public class PurchaseResponse
{
    public int PurchaseId { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PurchaseItemResponse> PurchaseItems { get; set; } = new();
}
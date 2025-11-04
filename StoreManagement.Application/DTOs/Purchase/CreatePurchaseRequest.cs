using System.Collections.Generic;

namespace StoreManagement.Application.DTOs.Purchase;

public class CreatePurchaseRequest
{
    public int? SupplierId { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseItemRequest> Items { get; set; } = new List<PurchaseItemRequest>();
}
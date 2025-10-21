using System.Collections.Generic;

namespace StoreManagement.Application.DTOs.Inventory;

public class CreateInventoryRequest
{
    public List<InventoryItemRequest> Items { get; set; } = new List<InventoryItemRequest>();
}

public class InventoryItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
namespace StoreManagement.Application.DTOs.Inventory;

public class CreateInventoryRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
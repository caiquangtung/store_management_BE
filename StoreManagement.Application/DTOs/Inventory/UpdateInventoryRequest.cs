namespace StoreManagement.Application.DTOs.Inventory;

public class UpdateInventoryRequest
{
    public int Quantity { get; set; }
    public bool SetToZero { get; set; } = false;  // Optional flag to set quantity = 0
}
namespace StoreManagement.Application.DTOs.InventoryAdjustment;

public class CreateAdjustmentRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
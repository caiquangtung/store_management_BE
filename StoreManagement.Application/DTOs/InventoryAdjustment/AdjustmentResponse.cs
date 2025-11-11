namespace StoreManagement.Application.DTOs.InventoryAdjustment;

public class AdjustmentResponse
{
    public int AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? StaffName { get; set; }  // Đổi từ UserName -> StaffName để rõ nghĩa hơn
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
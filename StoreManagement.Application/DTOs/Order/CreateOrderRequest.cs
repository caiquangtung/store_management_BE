namespace StoreManagement.Application.DTOs.Order;

public class CreateOrderRequest
{
    public int? CustomerId { get; set; }

    // ✅ Cho phép nhận PromoId (nếu có)
    public int? PromoId { get; set; }

    // ✅ Thông tin chi tiết các sản phẩm trong đơn
    public List<OrderItemRequest> OrderDetails { get; set; } = new();

    // ✅ Thông tin tổng tiền
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // UserId vẫn lấy từ JWT trong controller
}

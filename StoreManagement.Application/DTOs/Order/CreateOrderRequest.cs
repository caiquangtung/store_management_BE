namespace StoreManagement.Application.DTOs.Order;

public class CreateOrderRequest
{
    // LỰA CHỌN 1: Dành cho khách hàng đã có tài khoản (nếu có)
    public int? CustomerId { get; set; }

    // LỰA CHỌN 2: Dành cho khách vãng lai (từ form)
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }

    // Thông tin giỏ hàng từ SQLite (đã có)
    public List<OrderItemRequest> OrderDetails { get; set; } = new();

    // Thông tin thanh toán từ Form Checkout
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; } // Số tiền khách trả

    // Khuyến mãi (nếu có)
    // Tệp của bạn đã có PromoId, ta sẽ giữ lại
    public int? PromoId { get; set; }
    
    // Các trường Subtotal, DiscountAmount, TotalAmount đã bị xóa
    // vì chúng sẽ được tính toán an toàn ở phía máy chủ.
}
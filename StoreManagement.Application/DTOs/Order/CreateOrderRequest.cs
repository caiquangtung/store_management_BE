namespace StoreManagement.Application.DTOs.Order;

public class CreateOrderRequest
{
    public int? CustomerId { get; set; }

    // UserId sẽ được lấy từ JWT token trong controller
    // Không cần truyền từ client
}

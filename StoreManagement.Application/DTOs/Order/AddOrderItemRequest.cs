namespace StoreManagement.Application.DTOs.Order;

public class AddOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.Order;
using StoreManagement.Application.Services;
using StoreManagement.Domain.Enums;
using System.Security.Claims;

namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrStaff")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    // Helper method to get current user ID from JWT claims
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get paginated list of orders with filters and sorting
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] int? userId = null,
        [FromQuery] int? customerId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var (orders, totalCount) = await _orderService.GetAllPagedAsync(
                pagination.PageNumber, pagination.PageSize, status, userId, customerId, sortBy, sortDesc);

            var pagedResult = PagedResult<OrderResponse>.Create(orders, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PagedResult<OrderResponse>>.SuccessResponse(pagedResult, "Orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving orders");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving orders"));
        }
    }

    /// <summary>
    /// Get order details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Order not found"));
            }
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Order retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving order with ID {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving order"));
        }
    }

    /// <summary>
    /// Create new order (cart)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var userId = GetCurrentUserId();
            var order = await _orderService.CreateAsync(request, userId);

            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId },
                ApiResponse<OrderResponse>.SuccessResponse(order, "Order created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order creation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating order");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating order"));
        }
    }

    /// <summary>
    /// Update order information
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var order = await _orderService.UpdateAsync(id, request);
            if (order == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Order not found"));
            }

            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Order updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order update failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating order"));
        }
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        try
        {
            var success = await _orderService.CancelAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse.ErrorResponse("Order not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Order cancelled successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order cancellation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while cancelling order"));
        }
    }

    /// <summary>
    /// Add item to order
    /// </summary>
    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddOrderItem(int id, [FromBody] AddOrderItemRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var order = await _orderService.AddItemAsync(id, request);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Item added to order successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Add item failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding item to order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while adding item to order"));
        }
    }

    /// <summary>
    /// Update order item quantity
    /// </summary>
    [HttpPut("{id}/items/{itemId}")]
    public async Task<IActionResult> UpdateOrderItem(int id, int itemId, [FromBody] UpdateOrderItemRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var order = await _orderService.UpdateItemAsync(id, itemId, request);
            
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Order item updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Update item failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating item {ItemId} in order {OrderId}", itemId, id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating order item"));
        }
    }

    /// <summary>
    /// Delete order item
    /// </summary>
    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> DeleteOrderItem(int id, int itemId)
    {
        try
        {
            var order = await _orderService.DeleteItemAsync(id, itemId);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Order item deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Delete item failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting item {ItemId} from order {OrderId}", itemId, id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting order item"));
        }
    }

    /// <summary>
    /// Apply promotion to order
    /// </summary>
    [HttpPost("{id}/promotion")]
    public async Task<IActionResult> ApplyPromotion(int id, [FromBody] ApplyPromotionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var order = await _orderService.ApplyPromotionAsync(id, request);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Promotion applied successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Apply promotion failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while applying promotion to order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while applying promotion"));
        }
    }

    /// <summary>
    /// Remove promotion from order
    /// </summary>
    [HttpDelete("{id}/promotion")]
    public async Task<IActionResult> RemovePromotion(int id)
    {
        try
        {
            var order = await _orderService.RemovePromotionAsync(id);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Promotion removed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Remove promotion failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing promotion from order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while removing promotion"));
        }
    }

    /// <summary>
    /// Checkout order (process payment)
    /// </summary>
    [HttpPost("{id}/checkout")]
    public async Task<IActionResult> Checkout(int id, [FromBody] CheckoutRequest request)
    {   
      
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var order = await _orderService.CheckoutAsync(id, request);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order, "Order checked out successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Checkout failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking out order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing checkout"));
        }
    }
}

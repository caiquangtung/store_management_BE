using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.InventoryAdjustment;
using StoreManagement.Application.Services;
using System.Security.Claims;

namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/inventory/adjustments")] // Lồng vào inventory cho hợp lý
[Authorize(Policy = "AdminOnly")] // Chỉ Admin được điều chỉnh kho
public class InventoryAdjustmentsController : ControllerBase
{
    private readonly IInventoryAdjustmentService _adjustmentService;
    private readonly ILogger<InventoryAdjustmentsController> _logger;

    public InventoryAdjustmentsController(IInventoryAdjustmentService adjustmentService, ILogger<InventoryAdjustmentsController> logger)
    {
        _adjustmentService = adjustmentService;
        _logger = logger;
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateAdjustmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }
            
            var userId = GetCurrentUserId();
            var adjustment = await _adjustmentService.CreateAdjustmentAsync(request, userId);

            return Ok(ApiResponse<AdjustmentResponse>.SuccessResponse(adjustment, "Inventory adjusted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create adjustment");
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating adjustment");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating adjustment"));
        }
    }
    
    [HttpGet]
    [Authorize(Policy = "AdminOrStaff")] // Cho phép Staff xem lịch sử
    public async Task<IActionResult> GetAdjustments(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] int? productId = null)
    {
        var (items, totalCount) = await _adjustmentService.GetAllAdjustmentsPagedAsync(pagination.PageNumber, pagination.PageSize, productId);
        var pagedResult = PagedResult<AdjustmentResponse>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize);
        return Ok(ApiResponse<PagedResult<AdjustmentResponse>>.SuccessResponse(pagedResult, "Adjustments retrieved successfully"));
    }
}
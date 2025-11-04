using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.Purchase;
using StoreManagement.Application.Services;
using StoreManagement.Domain.Enums;
using System.Security.Claims;

namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrStaff")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ILogger<PurchasesController> _logger;

    public PurchasesController(IPurchaseService purchaseService, ILogger<PurchasesController> logger)
    {
        _purchaseService = purchaseService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<IActionResult> GetPurchases(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] PurchaseStatus? status = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var (items, totalCount) = await _purchaseService.GetAllPurchasesPagedAsync(
                pagination.PageNumber,
                pagination.PageSize,
                status,
                supplierId,
                searchTerm,
                sortBy,
                sortDesc);

            var pagedResult = PagedResult<PurchaseResponse>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PagedResult<PurchaseResponse>>.SuccessResponse(pagedResult, "Purchases retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchases");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving purchases"));
        }
    }
    
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var userId = GetCurrentUserId();
            var purchase = await _purchaseService.CreatePurchaseAsync(request, userId);
            
            return CreatedAtAction(nameof(GetPurchaseById), new { id = purchase.PurchaseId },
                ApiResponse<PurchaseResponse>.SuccessResponse(purchase, "Purchase created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating purchase"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPurchaseById(int id)
    {
        var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
        if (purchase == null)
            return NotFound(ApiResponse.ErrorResponse("Purchase not found"));
        
        return Ok(ApiResponse<PurchaseResponse>.SuccessResponse(purchase, "Purchase retrieved successfully"));
    }

    [HttpPost("{id}/confirm")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ConfirmPurchase(int id)
    {
        try
        {
            var purchase = await _purchaseService.ConfirmPurchaseAsync(id);
            return Ok(ApiResponse<PurchaseResponse>.SuccessResponse(purchase, "Purchase confirmed and inventory updated"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to confirm purchase {PurchaseId}", id);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming purchase {PurchaseId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while confirming purchase"));
        }
    }
    
    [HttpPost("{id}/cancel")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CancelPurchase(int id)
    {
        try
        {
            var purchase = await _purchaseService.CancelPurchaseAsync(id);
            return Ok(ApiResponse<PurchaseResponse>.SuccessResponse(purchase, "Purchase canceled successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to cancel purchase {PurchaseId}", id);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling purchase {PurchaseId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while canceling purchase"));
        }
    }
}
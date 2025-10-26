using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.Inventory;
using StoreManagement.Application.Services;
using System.Linq;

namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrStaff")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllInventory(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] int? productId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var (inventories, totalCount) = await _inventoryService.GetAllPagedAsync(
                pagination.PageNumber, pagination.PageSize, productId, sortBy, sortDesc);

            var pagedResult = PagedResult<InventoryResponse>.Create(inventories, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PagedResult<InventoryResponse>>.SuccessResponse(pagedResult, "Inventory retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving inventory");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving inventory"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventoryById(int id)
    {
        try
        {
            var inventory = await _inventoryService.GetByIdAsync(id);
            if (inventory == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Inventory not found"));
            }
            return Ok(ApiResponse<InventoryResponse>.SuccessResponse(inventory, "Inventory retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving inventory with ID {InventoryId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving inventory"));
        }
    }

    [HttpPost]
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryRequest request)
{
    try
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
        }

        var inventories = await _inventoryService.CreateAsync(request);  // UPDATED: Now returns List<InventoryResponse> for bulk
        if (inventories == null || !inventories.Any())
        {
            return BadRequest(ApiResponse.ErrorResponse("Failed to create inventory"));
        }

        // UPDATED: Return Ok with list for bulk, no CreatedAtAction (no single ID for redirect)
        return Ok(ApiResponse<List<InventoryResponse>>.SuccessResponse(inventories, "Inventory created/updated successfully"));
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Inventory creation failed: {Message}", ex.Message);
        return BadRequest(ApiResponse.ErrorResponse(ex.Message));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while creating inventory");
        return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating inventory"));
    }
}

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateInventory(int id, [FromBody] UpdateInventoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var inventory = await _inventoryService.UpdateAsync(id, request);
            if (inventory == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Inventory not found"));
            }

            return Ok(ApiResponse<InventoryResponse>.SuccessResponse(inventory, "Inventory updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Inventory update failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating inventory with ID {InventoryId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating inventory"));
        }
    }

    [HttpPut("{id}/set-zero")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetInventoryToZero(int id)
    {
        try
        {
            var result = await _inventoryService.SetQuantityToZeroAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Inventory not found"));
            }
            return Ok(ApiResponse.SuccessResponse("Inventory quantity set to zero successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting inventory to zero with ID {InventoryId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while setting inventory to zero"));
        }
    }

    [HttpGet("low-stock")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        try
        {
            var lowStock = await _inventoryService.GetLowStockAsync(threshold);
            return Ok(ApiResponse<IEnumerable<LowStockResponse>>.SuccessResponse(lowStock, "Low stock items retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Low stock query failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving low stock with threshold {Threshold}", threshold);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving low stock"));
        }
    }
}
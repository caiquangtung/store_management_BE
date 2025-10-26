using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.Suppliers;
using StoreManagement.Application.Services;
using System.Linq;
using StoreManagement.Domain.Enums;
namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrStaff")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var (suppliers, totalCount) = await _supplierService.GetAllPagedAsync(
                pagination.PageNumber, pagination.PageSize, status, searchTerm, sortBy, sortDesc);

            var pagedResult = PagedResult<SupplierResponse>.Create(suppliers, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PagedResult<SupplierResponse>>.SuccessResponse(pagedResult, "Suppliers retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving suppliers");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving suppliers"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        try
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }
            return Ok(ApiResponse<SupplierResponse>.SuccessResponse(supplier, "Supplier retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving supplier with ID {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving supplier"));
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var supplier = await _supplierService.CreateAsync(request);
            if (supplier == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to create supplier"));
            }

            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.SupplierId },
                ApiResponse<SupplierResponse>.SuccessResponse(supplier, "Supplier created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating supplier");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating supplier"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var supplier = await _supplierService.UpdateAsync(id, request);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            return Ok(ApiResponse<SupplierResponse>.SuccessResponse(supplier, "Supplier updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating supplier with ID {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating supplier"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        try
        {
            var result = await _supplierService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }
            return Ok(ApiResponse.SuccessResponse("Supplier deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting supplier with ID {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting supplier"));
        }
    }
}
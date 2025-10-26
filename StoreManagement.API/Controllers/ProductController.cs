using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.Products;
using StoreManagement.Application.Services;
using System.Linq;
using System.Globalization;
using StoreManagement.Domain.Enums;
namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrStaff")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            // Get paged products from service with database-level pagination and search
            var (products, totalCount) = await _productService.GetAllPagedAsync(
                pagination.PageNumber, pagination.PageSize, status, searchTerm, sortBy, sortDesc);

            var pagedResult = PagedResult<ProductResponse>.Create(products, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PagedResult<ProductResponse>>.SuccessResponse(pagedResult, "Products retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving products");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving products"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }
            return Ok(ApiResponse<ProductResponse>.SuccessResponse(product, "Product retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product with ID {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving product"));
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var product = await _productService.CreateAsync(request);
            if (product == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to create product"));
            }

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId },
                ApiResponse<ProductResponse>.SuccessResponse(product, "Product created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product creation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating product"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var product = await _productService.UpdateAsync(id, request);
            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            return Ok(ApiResponse<ProductResponse>.SuccessResponse(product, "Product updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product update failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product with ID {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating product"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }
            return Ok(ApiResponse.SuccessResponse("Product deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting product"));
        }
    }

    [HttpGet("abc-analysis")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<IActionResult> GetABCAnalysis([FromQuery] PaginationParameters pagination, [FromQuery] string? fromDate = null, [FromQuery] string? toDate = null)
    {
        try
        {
            // Parse date params null-safe
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;
            if (!string.IsNullOrEmpty(fromDate))
            {
                if (DateTime.TryParse(fromDate, out var parsedFrom))
                {
                    fromDateParsed = parsedFrom.Date;  // Set to start of day
                }
                else
                {
                    return BadRequest(ApiResponse.ErrorResponse("Invalid fromDate format. Use YYYY-MM-DD."));
                }
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                if (DateTime.TryParse(toDate, out var parsedTo))
                {
                    toDateParsed = parsedTo.Date.AddDays(1).AddTicks(-1);  // Set to end of day
                }
                else
                {
                    return BadRequest(ApiResponse.ErrorResponse("Invalid toDate format. Use YYYY-MM-DD."));
                }
            }

            // Validate fromDate <= toDate
            if (fromDateParsed.HasValue && toDateParsed.HasValue && fromDateParsed > toDateParsed)
            {
                return BadRequest(ApiResponse.ErrorResponse("fromDate must be before or equal to toDate."));
            }

            var abcAnalysis = await _productService.GetABCAnalysisAsync(fromDateParsed, toDateParsed, pagination.PageNumber, pagination.PageSize);
            var totalCount = abcAnalysis.Count();  // Or query total separately if needed
            var pagedResult = PagedResult<ABCProductResponse>.Create(abcAnalysis, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PagedResult<ABCProductResponse>>.SuccessResponse(pagedResult, "ABC Analysis retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving ABC analysis");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving ABC analysis"));
        }
    }
}
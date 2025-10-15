using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.Products;
using StoreManagement.Application.Services;
using System.Linq;

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
    public async Task<IActionResult> GetAllProducts([FromQuery] PaginationParameters pagination)
    {
        try
        {
            var products = await _productService.GetAllAsync();
            var totalCount = products.Count();
            var items = products
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToList();

            var pagedResult = PagedResult<ProductResponse>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize);
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
}
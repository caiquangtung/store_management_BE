using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.DTOs.Categories;
using StoreManagement.Application.Services;
using System.Linq;
using StoreManagement.Domain.Enums;
namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrStaff")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var (categories, totalCount) = await _categoryService.GetAllPagedAsync(
                pagination.PageNumber, pagination.PageSize, status, searchTerm, sortBy, sortDesc);

            var pagedResult = PagedResult<CategoryResponse>.Create(categories, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PagedResult<CategoryResponse>>.SuccessResponse(pagedResult, "Categories retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving categories");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving categories"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Category not found"));
            }
            return Ok(ApiResponse<CategoryResponse>.SuccessResponse(category, "Category retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving category with ID {CategoryId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving category"));
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var category = await _categoryService.CreateAsync(request);
            if (category == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to create category"));
            }

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId },
                ApiResponse<CategoryResponse>.SuccessResponse(category, "Category created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating category");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating category"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var category = await _categoryService.UpdateAsync(id, request);
            if (category == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Category not found"));
            }

            return Ok(ApiResponse<CategoryResponse>.SuccessResponse(category, "Category updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating category with ID {CategoryId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating category"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Category not found"));
            }
            return Ok(ApiResponse.SuccessResponse("Category deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting category with ID {CategoryId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting category"));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs.Promotion;
using StoreManagement.Application.Services;
using StoreManagement.API.Models;
using StoreManagement.API.Attributes;
using StoreManagement.Domain.Enums;

namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    /// <summary>
    /// Get all promotions with pagination and search
    /// </summary>
    [HttpGet]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<PagedResult<PromotionResponse>>>> GetPromotions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // Get all promotions from service
            var allPromotions = await _promotionService.GetPromotionsAsync(searchTerm);
            var promotionsList = allPromotions.ToList();

            // Apply pagination
            var totalCount = promotionsList.Count;
            var paginatedPromotions = promotionsList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Create paged result
            var pagedResult = new PagedResult<PromotionResponse>
            {
                Items = paginatedPromotions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(new ApiResponse<PagedResult<PromotionResponse>>
            {
                Success = true,
                Data = pagedResult,
                Message = "Promotions retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PagedResult<PromotionResponse>>
            {
                Success = false,
                Message = $"An error occurred while retrieving promotions: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get promotion by ID
    /// </summary>
    [HttpGet("{id}")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> GetPromotion(int id)
    {
        try
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound(new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "Promotion not found"
                });
            }

            return Ok(new ApiResponse<PromotionResponse>
            {
                Success = true,
                Data = promotion,
                Message = "Promotion retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = $"An error occurred while retrieving promotion: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get promotion by promo code
    /// </summary>
    [HttpGet("by-code/{promoCode}")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> GetPromotionByCode(string promoCode)
    {
        try
        {
            var promotion = await _promotionService.GetPromotionByCodeAsync(promoCode);
            if (promotion == null)
            {
                return NotFound(new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "Promotion not found"
                });
            }

            return Ok(new ApiResponse<PromotionResponse>
            {
                Success = true,
                Data = promotion,
                Message = "Promotion retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = $"An error occurred while retrieving promotion: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get active promotions
    /// </summary>
    [HttpGet("active")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PromotionResponse>>>> GetActivePromotions()
    {
        try
        {
            var activePromotions = await _promotionService.GetActivePromotionsAsync();
            return Ok(new ApiResponse<IEnumerable<PromotionResponse>>
            {
                Success = true,
                Data = activePromotions,
                Message = "Active promotions retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<PromotionResponse>>
            {
                Success = false,
                Message = $"An error occurred while retrieving active promotions: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Check if promo code exists
    /// </summary>
    [HttpGet("check-code/{promoCode}")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckPromoCodeExists(string promoCode)
    {
        try
        {
            var exists = await _promotionService.PromoCodeExistsAsync(promoCode);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = exists,
                Message = exists ? "Promo code exists" : "Promo code does not exist"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"An error occurred while checking promo code: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Validate promotion
    /// </summary>
    [HttpPost("validate")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<PromotionValidationResponse>>> ValidatePromotion([FromBody] ValidatePromotionRequest request)
    {
        try
        {
            var validationResult = await _promotionService.ValidatePromotionAsync(request);
            return Ok(new ApiResponse<PromotionValidationResponse>
            {
                Success = true,
                Data = validationResult,
                Message = validationResult.IsValid ? "Promotion is valid" : "Promotion validation failed"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PromotionValidationResponse>
            {
                Success = false,
                Message = $"An error occurred while validating promotion: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Calculate discount amount
    /// </summary>
    [HttpPost("calculate-discount")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<decimal>>> CalculateDiscount([FromBody] ValidatePromotionRequest request)
    {
        try
        {
            var validationResult = await _promotionService.ValidatePromotionAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<decimal>
                {
                    Success = false,
                    Data = 0,
                    Message = validationResult.Message
                });
            }

            return Ok(new ApiResponse<decimal>
            {
                Success = true,
                Data = validationResult.DiscountAmount,
                Message = "Discount calculated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<decimal>
            {
                Success = false,
                Message = $"An error occurred while calculating discount: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Create a new promotion
    /// </summary>
    [HttpPost]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        try
        {
            // Check if promo code already exists
            if (await _promotionService.PromoCodeExistsAsync(request.PromoCode))
            {
                return BadRequest(new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "Promo code already exists"
                });
            }

            var promotion = await _promotionService.CreatePromotionAsync(request);
            return CreatedAtAction(nameof(GetPromotion), new { id = promotion?.PromoId }, new ApiResponse<PromotionResponse>
            {
                Success = true,
                Data = promotion,
                Message = "Promotion created successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = $"An error occurred while creating promotion: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Update an existing promotion
    /// </summary>
    [HttpPut("{id}")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> UpdatePromotion(int id, [FromBody] UpdatePromotionRequest request)
    {
        try
        {
            if (id != request.PromoId)
            {
                return BadRequest(new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "ID mismatch"
                });
            }

            // Check if promotion exists
            var existingPromotion = await _promotionService.GetPromotionByIdAsync(id);
            if (existingPromotion == null)
            {
                return NotFound(new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "Promotion not found"
                });
            }

            // Check if promo code already exists for another promotion
            if (await _promotionService.PromoCodeExistsForOtherPromotionAsync(request.PromoCode, id))
            {
                return BadRequest(new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "Promo code already exists for another promotion"
                });
            }

            var promotion = await _promotionService.UpdatePromotionAsync(request);
            return Ok(new ApiResponse<PromotionResponse>
            {
                Success = true,
                Data = promotion,
                Message = "Promotion updated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = $"An error occurred while updating promotion: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Delete a promotion
    /// </summary>
    [HttpDelete("{id}")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePromotion(int id)
    {
        try
        {
            var result = await _promotionService.DeletePromotionAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Promotion not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Promotion deleted successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"An error occurred while deleting promotion: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Deactivate expired promotions
    /// </summary>
    [HttpPost("deactivate-expired")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateExpiredPromotions()
    {
        try
        {
            await _promotionService.DeactivateExpiredPromotionsAsync();
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Expired promotions deactivated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"An error occurred while deactivating expired promotions: {ex.Message}"
            });
        }
    }
}

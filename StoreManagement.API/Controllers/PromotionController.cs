using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs.Promotion;
using StoreManagement.Application.Services;
using StoreManagement.API.Models;

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
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<ActionResult<ApiResponse<PagedResult<PromotionResponse>>>> GetPromotions(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            // Get paged promotions from service with database-level pagination
            var (promotions, totalCount) = await _promotionService.GetPromotionsPagedAsync(
                pagination.PageNumber, pagination.PageSize, searchTerm, sortBy, sortDesc);

            var pagedResult = PagedResult<PromotionResponse>.Create(promotions, totalCount, pagination.PageNumber, pagination.PageSize);

            return Ok(new ApiResponse<PagedResult<PromotionResponse>>
            {
                Success = true,
                Data = pagedResult,
                Message = "Promotions retrieved successfully"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<PagedResult<PromotionResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving promotions"
            });
        }
    }

    /// <summary>
    /// Get promotion by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving promotion"
            });
        }
    }

    /// <summary>
    /// Get promotion by promo code
    /// </summary>
    [HttpGet("by-code/{promoCode}")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving promotion"
            });
        }
    }

    /// <summary>
    /// Get active promotions
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<PromotionResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving active promotions"
            });
        }
    }

    /// <summary>
    /// Check if promo code exists
    /// </summary>
    [HttpGet("check-code/{promoCode}")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while checking promo code"
            });
        }
    }

    /// <summary>
    /// Validate promotion
    /// </summary>
    [HttpPost("validate")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<PromotionValidationResponse>
            {
                Success = false,
                Message = "An error occurred while validating promotion"
            });
        }
    }

    /// <summary>
    /// Calculate discount amount
    /// </summary>
    [HttpPost("calculate-discount")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<decimal>
            {
                Success = false,
                Message = "An error occurred while calculating discount"
            });
        }
    }

    /// <summary>
    /// Create a new promotion
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = "An error occurred while creating promotion"
            });
        }
    }

    /// <summary>
    /// Update an existing promotion
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> UpdatePromotion(int id, [FromBody] UpdatePromotionRequest request)
    {
        try
        {
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

            var promotion = await _promotionService.UpdatePromotionAsync(id, request);
            return Ok(new ApiResponse<PromotionResponse>
            {
                Success = true,
                Data = promotion,
                Message = "Promotion updated successfully"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<PromotionResponse>
            {
                Success = false,
                Message = "An error occurred while updating promotion"
            });
        }
    }

    /// <summary>
    /// Delete a promotion
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting promotion"
            });
        }
    }

    /// <summary>
    /// Deactivate expired promotions
    /// </summary>
    [HttpPost("deactivate-expired")]
    [Authorize(Policy = "AdminOnly")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deactivating expired promotions"
            });
        }
    }
}

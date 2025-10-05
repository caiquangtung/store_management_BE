using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs.Auth;
using StoreManagement.Application.Services;
using StoreManagement.API.Models;
using FluentValidation;

namespace StoreManagement.API.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    /// <summary>
    /// Exchange a valid refresh token for a new access token and refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(ApiResponse<RefreshTokenResponse>.ValidationErrorResponse(new List<string> { "RefreshToken is required" }));
            }

            var response = await _authService.RefreshAsync(request);
            if (response == null)
            {
                return Unauthorized(new ApiResponse<RefreshTokenResponse>
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                });
            }

            return Ok(new ApiResponse<RefreshTokenResponse>
            {
                Success = true,
                Message = "Token refreshed",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, new ApiResponse<RefreshTokenResponse>
            {
                Success = false,
                Message = "An error occurred during token refresh"
            });
        }
    }

    /// <summary>
    /// Revoke a refresh token (logout)
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(new List<string> { "RefreshToken is required" }));
            }

            var revoked = await _authService.LogoutAsync(request.RefreshToken);
            if (!revoked)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Refresh token not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logged out"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during logout"
            });
        }
    }
    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<LoginResponse>.ValidationErrorResponse(errors));
            }

            // Authenticate user
            var loginResponse = await _authService.LoginAsync(request);
            if (loginResponse == null)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = loginResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for user: {Username}", request.Username);
            return StatusCode(500, new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }
}

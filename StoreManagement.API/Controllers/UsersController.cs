using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs.Users;
using StoreManagement.Application.Services;
using StoreManagement.API.Models;
using StoreManagement.API.Attributes;
using StoreManagement.Domain.Enums;
using System.Linq;

namespace StoreManagement.API.Controllers;

/// <summary>
/// Users controller for user management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin and Staff only)
    /// </summary>
    /// <returns>Paged list of users</returns>
    [HttpGet]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var (users, totalCount) = await _userService.GetAllPagedAsync(
                pagination.PageNumber, pagination.PageSize, status, role, searchTerm, sortBy, sortDesc);

            var pagedResult = PagedResult<UserResponse>.Create(users, totalCount, pagination.PageNumber, pagination.PageSize);

            return Ok(ApiResponse<PagedResult<UserResponse>>.SuccessResponse(pagedResult, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving users"));
        }
    }

    /// <summary>
    /// Get user by ID (Admin and Staff only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<UserResponse>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving user"));
        }
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user details</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var user = await _userService.CreateAsync(request);
            if (user == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to create user"));
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId },
                ApiResponse<UserResponse>.SuccessResponse(user, "User created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User creation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating user"));
        }
    }

    /// <summary>
    /// Update an existing user (Admin and Staff only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update request</param>
    /// <returns>Updated user details</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(errors));
            }

            var user = await _userService.UpdateAsync(id, request);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<UserResponse>.SuccessResponse(user, "User updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating user"));
        }
    }

    /// <summary>
    /// Delete a user (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse.SuccessResponse("User deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting user"));
        }
    }

    /// <summary>
    /// Check if username exists (All authenticated users)
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns>Existence result</returns>
    [HttpGet("check-username/{username}")]
    [Authorize(Policy = "AllRoles")]
    public async Task<IActionResult> CheckUsername(string username)
    {
        try
        {
            var exists = await _userService.UsernameExistsAsync(username);
            return Ok(ApiResponse<object>.SuccessResponse(new { exists }, "Username check completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking username {Username}", username);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while checking username"));
        }
    }

    /// <summary>
    /// Get user count statistics by role (Admin and Staff only)
    /// </summary>
    /// <returns>User count by role statistics</returns>
    [HttpGet("count-by-role")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<IActionResult> GetUserCountByRole()
    {
        try
        {
            var statistics = await _userService.GetUserCountByRoleAsync();
            return Ok(ApiResponse<UserCountByRoleResponse>.SuccessResponse(statistics, "User count by role retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user count by role");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving user count by role"));
        }
    }
}

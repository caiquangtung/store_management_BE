using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.DTOs.Users;

/// <summary>
/// Update user request DTO
/// </summary>
public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public UserRole? Role { get; set; }
    public string? NewPassword { get; set; }
}

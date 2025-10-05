using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.DTOs.Users;

/// <summary>
/// Create user request DTO
/// </summary>
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Staff;
}

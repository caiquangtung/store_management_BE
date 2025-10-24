using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.DTOs.Users;

/// <summary>
/// User response DTO
/// </summary>
public class UserResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public EntityStatus Status { get; set; }
}

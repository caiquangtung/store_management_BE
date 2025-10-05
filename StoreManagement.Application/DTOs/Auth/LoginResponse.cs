namespace StoreManagement.Application.DTOs.Auth;

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// User information in login response
/// </summary>
public class UserInfo
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
}

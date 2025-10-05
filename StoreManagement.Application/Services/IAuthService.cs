using StoreManagement.Application.DTOs.Auth;

namespace StoreManagement.Application.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(string refreshToken);
}

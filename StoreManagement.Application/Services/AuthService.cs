using StoreManagement.Application.DTOs.Auth;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Domain.Enums;
using StoreManagement.Application.Common.Interfaces;

namespace StoreManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IRefreshTokenStore refreshTokenStore)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // Find user by username
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        // Nếu user không tồn tại HOẶC user đang bị vô hiệu hóa, trả về null.
        if (user == null || user.Status == Domain.Enums.EntityStatus.Inactive)
        {
            return null; // User not found or is inactive
        }

        // Verify password
        if (!_passwordService.VerifyPassword(request.Password, user.Password))
        {
            return null; // Invalid password
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(
            user.UserId.ToString(),
            user.Username,
            user.Role.ToString()
        );

        // Calculate token expiration
        var expiresAt = DateTime.UtcNow.AddMinutes(60); // Should match JwtSettings.ExpireMinutes

        // Issue refresh token (in-memory)
        var refreshLifetime = TimeSpan.FromDays(7);
        var refreshToken = await _refreshTokenStore.IssueTokenAsync(user.UserId, refreshLifetime);
        var refreshExpiresAt = DateTime.UtcNow.Add(refreshLifetime);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            User = new UserInfo
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role.ToString()
            }
        };
    }

    public async Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request)
    {
        var userId = await _refreshTokenStore.ValidateTokenAsync(request.RefreshToken);
        if (!userId.HasValue)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return null;
        }

        // Rotate refresh token: revoke old, issue new
        await _refreshTokenStore.RevokeTokenAsync(request.RefreshToken);
        var refreshLifetime = TimeSpan.FromDays(7);
        var newRefreshToken = await _refreshTokenStore.IssueTokenAsync(user.UserId, refreshLifetime);
        var refreshExpiresAt = DateTime.UtcNow.Add(refreshLifetime);

        var token = _jwtService.GenerateToken(
            user.UserId.ToString(),
            user.Username,
            user.Role.ToString()
        );
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        return new RefreshTokenResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var result = await _refreshTokenStore.RevokeTokenAsync(refreshToken);
        return result;
    }
}

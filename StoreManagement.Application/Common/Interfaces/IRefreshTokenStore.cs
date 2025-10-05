namespace StoreManagement.Application.Common.Interfaces;

public interface IRefreshTokenStore
{
    Task<string> IssueTokenAsync(int userId, TimeSpan lifetime);
    Task<int?> ValidateTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<int> RevokeAllForUserAsync(int userId);
    Task<DateTime?> GetExpiryAsync(string refreshToken);
}



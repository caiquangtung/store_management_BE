using System.Security.Claims;

namespace StoreManagement.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string username, string role);
    ClaimsPrincipal? ValidateToken(string token);
}

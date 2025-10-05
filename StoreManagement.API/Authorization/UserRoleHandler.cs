using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using StoreManagement.Domain.Enums;
using System.Security.Claims;

namespace StoreManagement.API.Authorization;

/// <summary>
/// Authorization handler for user role requirements
/// </summary>
public class UserRoleHandler : AuthorizationHandler<UserRoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserRoleRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        // Get user role from claims
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
        {
            return Task.CompletedTask;
        }

        // Parse role from string and validate it's a valid UserRole
        if (Enum.TryParse<UserRole>(roleClaim, out var userRole) && IsValidUserRole(userRole))
        {
            // Check if user role is in allowed roles
            if (requirement.AllowedRoles.Contains(userRole))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates if the UserRole is one of the allowed roles
    /// </summary>
    /// <param name="role">Role to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValidUserRole(UserRole role)
    {
        return role == UserRole.Admin || role == UserRole.Staff;
    }
}

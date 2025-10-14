using Microsoft.AspNetCore.Authorization;
using StoreManagement.Domain.Enums;

namespace StoreManagement.API.Attributes;

/// <summary>
/// Custom authorization attribute for role-based access control
/// </summary>
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params UserRole[] roles)
    {
        // Map to predefined policy names
        Policy = GetPolicyName(roles);
    }

    private static string GetPolicyName(UserRole[] roles)
    {
        // Sort roles for consistent policy name
        var sortedRoles = roles.OrderBy(r => r.ToString()).ToArray();

        if (sortedRoles.Length == 1)
        {
            return sortedRoles[0] == UserRole.Admin ? "AdminOnly" : "AllRoles";
        }

        if (sortedRoles.Length == 2 && sortedRoles.Contains(UserRole.Admin) && sortedRoles.Contains(UserRole.Staff))
        {
            return "AdminOrStaff";
        }

        // Fallback: create policy name from roles
        var roleNames = sortedRoles.Select(r => r.ToString()).ToArray();
        return string.Join("Or", roleNames);
    }
}

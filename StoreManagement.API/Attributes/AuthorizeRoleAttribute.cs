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
        var roleNames = roles.Select(r => r.ToString()).ToArray();
        Policy = string.Join(",", roleNames);
    }
}

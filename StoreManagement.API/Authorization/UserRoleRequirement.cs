using Microsoft.AspNetCore.Authorization;
using StoreManagement.Domain.Enums;

namespace StoreManagement.API.Authorization;

/// <summary>
/// Authorization requirement for user roles
/// </summary>
public class UserRoleRequirement : IAuthorizationRequirement
{
    public UserRole[] AllowedRoles { get; }

    public UserRoleRequirement(params UserRole[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

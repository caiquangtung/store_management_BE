using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.DTOs.Users;

/// <summary>
/// Response DTO for user count by role statistics
/// </summary>
public class UserCountByRoleResponse
{
    public Dictionary<UserRole, int> RoleCounts { get; set; } = new();

    public int TotalCount { get; set; }

}

using StoreManagement.Application.DTOs.Users;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(int id);
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<(IEnumerable<UserResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, EntityStatus? status = null, UserRole? role = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false);
    Task<UserResponse?> CreateAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> UsernameExistsAsync(string username);
    Task<UserCountByRoleResponse> GetUserCountByRoleAsync();
}

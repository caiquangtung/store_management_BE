using StoreManagement.Application.DTOs.Users;

namespace StoreManagement.Application.Services;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(int id);
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<UserResponse?> CreateAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> UsernameExistsAsync(string username);
}

using AutoMapper;
using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.DTOs.Users;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Domain.Enums;
using System.Linq.Expressions;

namespace StoreManagement.Application.Services;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? _mapper.Map<UserResponse>(user) : null;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponse>>(users);
    }

    public async Task<(IEnumerable<UserResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, UserRole? role = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false)
    {
        // Build filter expression
        Expression<Func<User, bool>>? filter = null;

        if (role.HasValue && !string.IsNullOrEmpty(searchTerm))
        {
            // Both filters: role AND search
            filter = u => u.Role == role.Value &&
                         (u.Username.Contains(searchTerm) ||
                          (u.FullName != null && u.FullName.Contains(searchTerm)));
        }
        else if (role.HasValue)
        {
            // Only role filter
            filter = u => u.Role == role.Value;
        }
        else if (!string.IsNullOrEmpty(searchTerm))
        {
            // Only search filter
            filter = u => u.Username.Contains(searchTerm) ||
                         (u.FullName != null && u.FullName.Contains(searchTerm));
        }

        // Build order expression with whitelist and stable tie-breaker by UserId
        Expression<Func<User, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
        {
            "id" => u => u.UserId,
            "username" => u => u.Username,
            "fullname" => u => u.FullName ?? string.Empty,
            "role" => u => u.Role,
            _ => u => u.UserId
        };

        Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = q =>
        {
            var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
            // Tie-breaker for stable pagination
            return sortDesc ? ordered.ThenByDescending(u => u.UserId) : ordered.ThenBy(u => u.UserId);
        };

        var (items, totalCount) = await _userRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter,
            orderBy);

        var mappedItems = _mapper.Map<IEnumerable<UserResponse>>(items);
        return (mappedItems, totalCount);
    }

    public async Task<UserResponse?> CreateAsync(CreateUserRequest request)
    {
        // Check if username already exists
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Hash password
        var hashedPassword = _passwordService.HashPassword(request.Password);

        // Create user entity
        var user = new User
        {
            Username = request.Username,
            Password = hashedPassword,
            FullName = request.FullName,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserResponse>(createdUser);
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.FullName))
        {
            user.FullName = request.FullName;
        }

        if (request.Role.HasValue)
        {
            user.Role = request.Role.Value;
        }

        // UPDATED: Handle username update if provided
        if (!string.IsNullOrEmpty(request.Username))
        {
            if (await _userRepository.UsernameExistsAsync(request.Username) && request.Username != user.Username)
            {
                throw new InvalidOperationException("Username already exists");
            }
            user.Username = request.Username;
        }
        
        // Update password if provided
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            user.Password = _passwordService.HashPassword(request.NewPassword);
        }

        var updatedUser = await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserResponse>(updatedUser);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _userRepository.UsernameExistsAsync(username);
    }

    public async Task<UserCountByRoleResponse> GetUserCountByRoleAsync()
    {
        var users = await _userRepository.GetAllAsync();

        var roleCounts = users
            .GroupBy(u => u.Role)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalCount = users.Count();

        return new UserCountByRoleResponse
        {
            RoleCounts = roleCounts,
            TotalCount = totalCount
        };
    }
}

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

    public async Task<(IEnumerable<UserResponse> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, 
        int pageSize, 
        EntityStatus? status = null, 
        UserRole? role = null, 
        string? searchTerm = null, 
        string? sortBy = null, 
        bool sortDesc = false)
    {
        // Xây dựng một biểu thức lọc duy nhất xử lý tất cả các tham số
        Expression<Func<User, bool>> filter = u =>
            (!status.HasValue || u.Status == status.Value) &&
            (!role.HasValue || u.Role == role.Value) &&
            (string.IsNullOrEmpty(searchTerm) || 
                u.Username.Contains(searchTerm) || 
                (u.FullName != null && u.FullName.Contains(searchTerm)));

        // Logic sắp xếp (không thay đổi)
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
            return sortDesc ? ordered.ThenByDescending(u => u.UserId) : ordered.ThenBy(u => u.UserId);
        };

        // Gọi repository với biểu thức lọc đã được xây dựng
        var (items, totalCount) = await _userRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter, // <-- Sử dụng biểu thức lọc mới
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
        
        if (request.Status.HasValue)
        {
            user.Status = request.Status.Value;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserResponse>(updatedUser);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Do đã có Global Query Filter, GetByIdAsync sẽ không tìm thấy user đã "deleted".
        // Để xóa mềm một user, chúng ta cần bỏ qua filter này tạm thời.
        var user = await _userRepository.FindAsync(u => u.UserId == id);
        var userToUpdate = user.FirstOrDefault();

        if (userToUpdate == null)
        {
            return false;
        }
        userToUpdate.Status = EntityStatus.Deleted;

        await _userRepository.UpdateAsync(userToUpdate);
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

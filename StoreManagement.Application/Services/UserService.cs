using AutoMapper;
using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.DTOs.Users;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;

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
}

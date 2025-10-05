using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.Repositories;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(StoreDbContext context) : base(context)
    {
    }


    public async Task<User?> GetByEmailAsync(string email)
    {
        // Note: User entity doesn't have Email field yet, implementing for future use
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        // Note: User entity doesn't have Email field yet, implementing for future use
        return await _context.Users.AnyAsync(u => u.Username == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

}

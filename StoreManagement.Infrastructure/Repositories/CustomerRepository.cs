using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(StoreDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Customers
            .AnyAsync(c => c.Email == email);
    }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone);
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        return await _context.Customers
            .AnyAsync(c => c.Phone == phone);
    }
}

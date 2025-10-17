using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<bool> PhoneExistsAsync(string phone);
}

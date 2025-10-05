using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}

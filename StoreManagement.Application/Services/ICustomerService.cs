using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.DTOs.Customer;

namespace StoreManagement.Application.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponse>> GetCustomersAsync(string? searchTerm = null);
    Task<CustomerResponse?> GetCustomerByIdAsync(int customerId);
    Task<CustomerResponse?> GetCustomerByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CustomerExistsAsync(int customerId);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerResponse?> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(int customerId);
}

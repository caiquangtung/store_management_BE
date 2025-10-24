using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.DTOs.Customer;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponse>> GetCustomersAsync(string? searchTerm = null);
    Task<(IEnumerable<CustomerResponse> Items, int TotalCount)> GetCustomersPagedAsync(
        int pageNumber, int pageSize, EntityStatus? status = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false);
    Task<CustomerResponse?> GetCustomerByIdAsync(int customerId);
    Task<CustomerResponse?> GetCustomerByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<CustomerResponse?> GetCustomerByPhoneAsync(string phone);
    Task<bool> PhoneExistsAsync(string phone);
    Task<bool> CustomerExistsAsync(int customerId);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerResponse?> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(int customerId);
}

using AutoMapper;
using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.DTOs.Customer;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;

namespace StoreManagement.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CustomerResponse>> GetCustomersAsync(string? searchTerm = null)
    {
        IEnumerable<Customer> customers;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            customers = await _customerRepository.FindAsync(c =>
                c.Name.Contains(searchTerm) ||
                (c.Email != null && c.Email.Contains(searchTerm)) ||
                (c.Phone != null && c.Phone.Contains(searchTerm)));
        }
        else
        {
            customers = await _customerRepository.GetAllAsync();
        }

        return _mapper.Map<IEnumerable<CustomerResponse>>(customers.OrderBy(c => c.Name));
    }

    public async Task<(IEnumerable<CustomerResponse> Items, int TotalCount)> GetCustomersPagedAsync(
        int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDesc = false)
    {
        // Build filter expression
        Expression<Func<Customer, bool>>? filter = null;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            filter = c => c.Name.Contains(searchTerm) ||
                         (c.Email != null && c.Email.Contains(searchTerm)) ||
                         (c.Phone != null && c.Phone.Contains(searchTerm));
        }

        Expression<Func<Customer, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
        {
            "id" => c => c.CustomerId,
            "name" => c => c.Name,
            "email" => c => c.Email ?? string.Empty,
            "phone" => c => c.Phone ?? string.Empty,
            _ => c => c.CustomerId
        };

        Func<IQueryable<Customer>, IOrderedQueryable<Customer>> orderBy = q =>
        {
            var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
            return sortDesc ? ordered.ThenByDescending(c => c.CustomerId) : ordered.ThenBy(c => c.CustomerId);
        };

        var (items, totalCount) = await _customerRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter,
            orderBy);

        // Map to response DTOs
        var mappedItems = _mapper.Map<IEnumerable<CustomerResponse>>(items);

        return (mappedItems, totalCount);
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        return customer != null ? _mapper.Map<CustomerResponse>(customer) : null;
    }

    public async Task<CustomerResponse?> GetCustomerByEmailAsync(string email)
    {
        var customer = await _customerRepository.GetByEmailAsync(email);
        return customer != null ? _mapper.Map<CustomerResponse>(customer) : null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _customerRepository.EmailExistsAsync(email);
    }

    public async Task<CustomerResponse?> GetCustomerByPhoneAsync(string phone)
    {
        var customer = await _customerRepository.GetByPhoneAsync(phone);
        return customer != null ? _mapper.Map<CustomerResponse>(customer) : null;
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        return await _customerRepository.PhoneExistsAsync(phone);
    }

    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        return customer != null;
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var customer = _mapper.Map<Customer>(request);
        var createdCustomer = await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();
        return _mapper.Map<CustomerResponse>(createdCustomer);
    }

    public async Task<CustomerResponse?> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(id);
        if (existingCustomer == null)
            return null;

        _mapper.Map(request, existingCustomer);
        var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
        await _customerRepository.SaveChangesAsync();
        return _mapper.Map<CustomerResponse>(updatedCustomer);
    }

    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            return false;

        await _customerRepository.DeleteAsync(customer);
        await _customerRepository.SaveChangesAsync();
        return true;
    }
}
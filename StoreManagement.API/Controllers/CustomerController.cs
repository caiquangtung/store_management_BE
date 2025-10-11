using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs.Customer;
using StoreManagement.Application.Services;
using StoreManagement.API.Models;
using StoreManagement.API.Attributes;
using StoreManagement.Domain.Enums;

namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Get all customers with pagination and search
    /// </summary>
    [HttpGet]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerResponse>>>> GetCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // Get all customers from service
            var allCustomers = await _customerService.GetCustomersAsync(searchTerm);
            var customersList = allCustomers.ToList();

            // Apply pagination
            var totalCount = customersList.Count;
            var paginatedCustomers = customersList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Create paged result
            var pagedResult = new PagedResult<CustomerResponse>
            {
                Items = paginatedCustomers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(new ApiResponse<PagedResult<CustomerResponse>>
            {
                Success = true,
                Data = pagedResult,
                Message = "Customers retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PagedResult<CustomerResponse>>
            {
                Success = false,
                Message = $"An error occurred while retrieving customers: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetCustomer(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Customer not found"
                });
            }

            return Ok(new ApiResponse<CustomerResponse>
            {
                Success = true,
                Data = customer,
                Message = "Customer retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = $"An error occurred while retrieving customer: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetCustomerByEmail(string email)
    {
        try
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
            {
                return NotFound(new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Customer not found"
                });
            }

            return Ok(new ApiResponse<CustomerResponse>
            {
                Success = true,
                Data = customer,
                Message = "Customer retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = $"An error occurred while retrieving customer: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Check if email exists
    /// </summary>
    [HttpGet("check-email/{email}")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists(string email)
    {
        try
        {
            var exists = await _customerService.EmailExistsAsync(email);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = exists,
                Message = exists ? "Email exists" : "Email does not exist"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"An error occurred while checking email: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            // Check if email already exists
            if (!string.IsNullOrEmpty(request.Email) && await _customerService.EmailExistsAsync(request.Email))
            {
                return BadRequest(new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Email already exists"
                });
            }

            var customer = await _customerService.CreateCustomerAsync(request);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, new ApiResponse<CustomerResponse>
            {
                Success = true,
                Data = customer,
                Message = "Customer created successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = $"An error occurred while creating customer: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    [AuthorizeRole(UserRole.Staff, UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            if (id != request.CustomerId)
            {
                return BadRequest(new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "ID mismatch"
                });
            }

            // Check if customer exists
            if (!await _customerService.CustomerExistsAsync(id))
            {
                return NotFound(new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Customer not found"
                });
            }

            // Check if email already exists for another customer
            if (!string.IsNullOrEmpty(request.Email))
            {
                var existingCustomer = await _customerService.GetCustomerByEmailAsync(request.Email);
                if (existingCustomer != null && existingCustomer.CustomerId != id)
                {
                    return BadRequest(new ApiResponse<CustomerResponse>
                    {
                        Success = false,
                        Message = "Email already exists for another customer"
                    });
                }
            }

            var customer = await _customerService.UpdateCustomerAsync(request);
            return Ok(new ApiResponse<CustomerResponse>
            {
                Success = true,
                Data = customer,
                Message = "Customer updated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = $"An error occurred while updating customer: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCustomer(int id)
    {
        try
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Customer not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Customer deleted successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"An error occurred while deleting customer: {ex.Message}"
            });
        }
    }
}
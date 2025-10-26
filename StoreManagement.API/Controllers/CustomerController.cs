using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs.Customer;
using StoreManagement.Application.Services;
using StoreManagement.API.Models;
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
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerResponse>>>> GetCustomers(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            // Get paged customers from service with database-level pagination
            var (customers, totalCount) = await _customerService.GetCustomersPagedAsync(
                pagination.PageNumber, pagination.PageSize, status, searchTerm, sortBy, sortDesc);

            var pagedResult = PagedResult<CustomerResponse>.Create(customers, totalCount, pagination.PageNumber, pagination.PageSize);

            return Ok(new ApiResponse<PagedResult<CustomerResponse>>
            {
                Success = true,
                Data = pagedResult,
                Message = "Customers retrieved successfully"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<PagedResult<CustomerResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving customers"
            });
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving customer"
            });
        }
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving customer"
            });
        }
    }

    /// <summary>
    /// Check if email exists
    /// </summary>
    [HttpGet("check-email/{email}")]
    [Authorize(Policy = "AdminOrStaff")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while checking email"
            });
        }
    }

    /// <summary>
    /// Get customer by phone number
    /// </summary>
    [HttpGet("by-phone/{phone}")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetCustomerByPhone(string phone)
    {
        try
        {
            var customer = await _customerService.GetCustomerByPhoneAsync(phone);
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving customer"
            });
        }
    }

    /// <summary>
    /// Check if phone number exists
    /// </summary>
    [HttpGet("check-phone/{phone}")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckPhoneExists(string phone)
    {
        try
        {
            var exists = await _customerService.PhoneExistsAsync(phone);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = exists,
                Message = exists ? "Phone number exists" : "Phone number does not exist"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while checking phone number"
            });
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOrStaff")]
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

            // Check if phone already exists
            if (!string.IsNullOrEmpty(request.Phone) && await _customerService.PhoneExistsAsync(request.Phone))
            {
                return BadRequest(new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Phone number already exists"
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = "An error occurred while creating customer"
            });
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOrStaff")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
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

            // Check if phone already exists for another customer
            if (!string.IsNullOrEmpty(request.Phone))
            {
                var existingCustomer = await _customerService.GetCustomerByPhoneAsync(request.Phone);
                if (existingCustomer != null && existingCustomer.CustomerId != id)
                {
                    return BadRequest(new ApiResponse<CustomerResponse>
                    {
                        Success = false,
                        Message = "Phone number already exists for another customer"
                    });
                }
            }

            var customer = await _customerService.UpdateCustomerAsync(id, request);
            return Ok(new ApiResponse<CustomerResponse>
            {
                Success = true,
                Data = customer,
                Message = "Customer updated successfully"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<CustomerResponse>
            {
                Success = false,
                Message = "An error occurred while updating customer"
            });
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting customer"
            });
        }
    }
}
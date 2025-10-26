# Customer API Implementation

## Overview

This document describes the implementation of the Customer API for the Store Management system. The Customer API provides full CRUD operations for managing customer information.

## Architecture

### Layers

- **Domain Layer**: Customer entity and ICustomerRepository interface
- **Application Layer**: Customer DTOs, services, validators, and AutoMapper profiles
- **Infrastructure Layer**: CustomerRepository implementation and database configuration
- **API Layer**: CustomerController with REST endpoints

## Implementation Details

### 1. Domain Layer

#### Customer Entity

```csharp
public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active; 

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

#### ICustomerRepository Interface

```csharp
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<bool> PhoneExistsAsync(string phone);
}
```

### 2. Application Layer

#### DTOs

- **CreateCustomerRequest**: For creating new customers
- **UpdateCustomerRequest**: For updating existing customers
- **CustomerResponse**: For returning customer data

#### Validators

- **CreateCustomerRequestValidator**: Validates customer creation data
- **UpdateCustomerRequestValidator**: Validates customer update data

#### Services

- **ICustomerService**: Interface defining customer operations
- **CustomerService**: Implementation of customer business logic

#### AutoMapper Profile

- **CustomerMappingProfile**: Maps between Customer entity and DTOs

### 3. Infrastructure Layer

#### CustomerRepository

```csharp
public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
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
```

#### Database Configuration

Customer entity is configured in StoreDbContext with proper column mappings and constraints.

### 4. API Layer

#### CustomerController Endpoints

##### GET /api/customer

- **Description**: Get all customers with pagination and search
- **Authorization**: Staff, Admin
- **Parameters**:
  - `pageNumber` (optional, default: 1)
  - `pageSize` (optional, default: 10, max: 100)
  - `searchTerm` (optional): Search by name, email, or phone
  - `status` (optional, Enum: Active, Inactive): Filter by customer status <-- CẬP NHẬT
- **Response**: PagedResult<CustomerResponse>

##### GET /api/customer/{id}

- **Description**: Get customer by ID
- **Authorization**: Staff, Admin
- **Parameters**: `id` (customer ID)
- **Response**: CustomerResponse

##### GET /api/customer/by-email/{email}

- **Description**: Get customer by email
- **Authorization**: Staff, Admin
- **Parameters**: `email` (customer email)
- **Response**: CustomerResponse

##### GET /api/customer/check-email/{email}

- **Description**: Check if email exists
- **Authorization**: Staff, Admin
- **Parameters**: `email` (email to check)
- **Response**: boolean

##### GET /api/customer/by-phone/{phone}

- **Description**: Get customer by phone number
- **Authorization**: Staff, Admin
- **Parameters**: `phone` (customer phone number)
- **Response**: CustomerResponse

##### GET /api/customer/check-phone/{phone}

- **Description**: Check if phone number exists
- **Authorization**: Staff, Admin
- **Parameters**: `phone` (phone number to check)
- **Response**: boolean

##### POST /api/customer

- **Description**: Create new customer
- **Authorization**: Staff, Admin
- **Body**: CreateCustomerRequest
- **Response**: CustomerResponse (201 Created)

##### PUT /api/customer/{id}

- **Description**: Update existing customer, including status (Active/Inactive)
- **Authorization**: Staff, Admin
- **Parameters**: `id` (customer ID)
- **Body**: UpdateCustomerRequest
- **Response**: CustomerResponse

##### DELETE /api/customer/{id}

- **Description**: Soft delete a customer (sets status to 'Deleted') 
- **Authorization**: Admin only
- **Parameters**: `id` (customer ID)
- **Response**: boolean

## Key Features

### 1. Pagination

- Pagination is handled at the controller level
- Service returns all matching records, controller applies pagination
- Supports page number and page size parameters
- Maximum page size limit of 100 items

### 2. Search Functionality

- Search by customer name, email, or phone number
- Case-insensitive search
- Partial matching support

### 3. Email and Phone Validation

- Email uniqueness validation
- Phone number uniqueness validation
- Proper email format validation
- Proper phone format validation
- Check for existing emails and phone numbers before creation/update

### 4. Authorization

- Role-based access control
- Staff and Admin can perform most operations
- Only Admin can delete customers

### 5. Error Handling

- Comprehensive exception handling
- Proper HTTP status codes
- Meaningful error messages

## Database Schema

### Customers Table

```sql
CREATE TABLE customers (
    customer_id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(100),
    address VARCHAR(200),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Relationships

- Customer has many Orders (one-to-many)
- Customer can have multiple Payments through Orders
- Customer can have multiple Products through OrderItems

## Validation Rules

### CreateCustomerRequest

- Name: Required, max 100 characters
- Phone: Optional, max 20 characters, valid phone format
- Email: Optional, valid email format, max 100 characters
- Address: Optional, max 200 characters

### UpdateCustomerRequest

- CustomerId: Required, must be greater than 0
- Same validation rules as CreateCustomerRequest

## Service Registration

The following services are registered in the DI container:

- `ICustomerRepository` → `CustomerRepository`
- `ICustomerService` → `CustomerService`
- Customer validators
- Customer AutoMapper profile

## Testing Considerations

### Unit Tests

- Test CustomerService business logic
- Test CustomerRepository data access
- Test CustomerController endpoints
- Test validators

### Integration Tests

- Test complete API workflows
- Test database operations
- Test authorization policies

## Future Enhancements

1. **Customer Analytics**: Add endpoints for customer statistics
2. **Bulk Operations**: Support for bulk customer import/export
3. **Customer Groups**: Add customer categorization
4. **Advanced Search**: Add filtering by date ranges, order history
5. **Audit Trail**: Track customer data changes
6. **Customer Notes**: Add ability to store notes about customers

## Configuration

### Connection String

Ensure the database connection string is properly configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StoreManagement;Uid=root;Pwd=password;"
  }
}
```

### JWT Settings

Customer API endpoints require JWT authentication. Ensure JWT settings are configured:

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key",
    "Issuer": "StoreManagement",
    "Audience": "StoreManagement",
    "ExpiryMinutes": 60
  }
}
```

## Usage Examples

### Create Customer

```bash
POST /api/customer
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "name": "John Doe",
  "phone": "+1234567890",
  "email": "john.doe@example.com",
  "address": "123 Main St, City, State"
}
```

### Get Customers with Pagination

```bash
GET /api/customer?pageNumber=1&pageSize=10&searchTerm=john
Authorization: Bearer <jwt-token>
```

### Update Customer

```bash
PUT /api/customer/1
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "customerId": 1,
  "name": "John Smith",
  "phone": "+1234567890",
  "email": "john.smith@example.com",
  "address": "456 Oak Ave, City, State"
}
```

### Get Customer by Phone

```bash
GET /api/customer/by-phone/+1234567890
Authorization: Bearer <jwt-token>
```

### Check Phone Number

```bash
GET /api/customer/check-phone/+1234567890
Authorization: Bearer <jwt-token>
```

### Filter Parameters

- `searchTerm` (string, optional): Search across multiple fields
- `status` (EntityStatus enum, optional): Filter by status (Active, Inactive) 

**Example Requests:**

```bash
# Search by name
GET /api/customer?pageNumber=1&pageSize=10&searchTerm=john

# Filter for inactive customers
GET /api/customer?status=Inactive 

# Search for active customers by email
GET /api/customer?status=Active&searchTerm=@gmail.com 
```
## Conclusion

The Customer API implementation follows clean architecture principles with proper separation of concerns, comprehensive validation, role-based authorization, and robust error handling. The pagination is handled at the controller level for better performance and flexibility.

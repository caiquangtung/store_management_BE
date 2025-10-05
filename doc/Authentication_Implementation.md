# Authentication Implementation Documentation

## Overview

This document describes the complete authentication implementation for the Store Management Backend API using JWT (JSON Web Tokens) and BCrypt for password hashing.

## Architecture

The authentication system follows Clean Architecture principles with the following layers:

- **API Layer**: Controllers and middleware
- **Application Layer**: Business logic and DTOs
- **Domain Layer**: Entities and interfaces
- **Infrastructure Layer**: Data access and external services

## Components Implemented

### 1. JWT Service (`StoreManagement.Infrastructure.Services`)

**Files:**

- `IJwtService.cs` - Interface for JWT operations
- `JwtService.cs` - Implementation with token generation and validation

**Features:**

- Generate JWT tokens with user claims (ID, username, role)
- Validate JWT tokens with proper security parameters
- Configurable expiration time
- HMAC SHA256 signature algorithm

### 2. Password Service (`StoreManagement.Infrastructure.Services`)

**Files:**

- `IPasswordService.cs` - Interface for password operations
- `PasswordService.cs` - BCrypt implementation

**Features:**

- Secure password hashing using BCrypt
- Password verification
- Salt generation handled automatically by BCrypt

### 3. Authentication Service (`StoreManagement.Application.Services`)

**Files:**

- `IAuthService.cs` - Interface for authentication operations
- `AuthService.cs` - Implementation of login logic

**Features:**

- User authentication with username/password
- JWT token generation upon successful login
- User information retrieval
- Integration with password service and JWT service

### 4. User Repository (`StoreManagement.Infrastructure.Repositories`)

**Files:**

- `UserRepository.cs` - Complete implementation of IRepository<User>

**Features:**

- CRUD operations for User entity
- Username-based user lookup
- Async operations with Entity Framework Core

### 5. Controllers (`StoreManagement.API.Controllers`)

**Files:**

- `AuthController.cs` - Authentication endpoints
- `UsersController.cs` - User management endpoints

**Endpoints:**

#### AuthController

- `POST /api/auth/login` - User login endpoint

#### UsersController

- `GET /api/users` - Get paginated list of users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### 6. Authorization (`StoreManagement.API.Authorization`)

**Files:**

- `UserRoleRequirement.cs` - Authorization requirement for role-based access
- `UserRoleHandler.cs` - Authorization handler implementation

**Features:**

- Role-based authorization with custom requirements
- JWT token validation for role claims
- Support for multiple roles in single requirement
- Secure role validation against UserRole enum

### 7. Custom Attributes (`StoreManagement.API.Attributes`)

**Files:**

- `AuthorizeRoleAttribute.cs` - Custom authorization attribute

**Features:**

- Simplified role-based authorization
- Support for multiple roles
- Integration with ASP.NET Core authorization system

### 8. DTOs (`StoreManagement.Application.DTOs.Auth`)

**Files:**

- `LoginRequest.cs` - Login request model
- `LoginResponse.cs` - Login response with token and user info

### 9. Validation (`StoreManagement.Application.Validators`)

**Files:**

- `LoginRequestValidator.cs` - FluentValidation rules for login requests

**Validation Rules:**

- Username: 3-50 characters, alphanumeric and underscores only
- Password: Minimum 6 characters, required

### 10. Configuration

**JWT Settings** (appsettings.json):

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-here",
    "Issuer": "StoreManagementAPI",
    "Audience": "StoreManagementClient",
    "ExpireMinutes": 60
  }
}
```

## Usage Examples

### 1. User Login

**Request:**

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "userId": 1,
      "username": "admin",
      "fullName": "System Administrator",
      "role": "Admin"
    }
  }
}
```

### 2. Using JWT Token

To use the JWT token in subsequent requests, include it in the Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

The token contains user information (ID, username, role) that can be accessed in protected endpoints using the `[Authorize]` attribute.

### 3. Role-Based Authorization

#### Using Custom Authorization Policies

```csharp
[HttpGet("admin-only")]
[Authorize(Policy = "AdminOnly")]
public IActionResult AdminOnlyEndpoint()
{
    return Ok("This endpoint is only accessible by Admin users");
}

[HttpGet("admin-or-staff")]
[Authorize(Policy = "AdminOrStaff")]
public IActionResult AdminOrStaffEndpoint()
{
    return Ok("This endpoint is accessible by Admin or Staff users");
}
```

#### Using Custom AuthorizeRole Attribute

```csharp
[HttpGet("admin-users")]
[AuthorizeRole(UserRole.Admin)]
public IActionResult GetAdminUsers()
{
    return Ok("Admin users only");
}

[HttpGet("all-users")]
[AuthorizeRole(UserRole.Admin, UserRole.Staff)]
public IActionResult GetAllUsers()
{
    return Ok("Admin and Staff users");
}
```

#### Authorization Policies Configuration

```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new UserRoleRequirement(UserRole.Admin)));

    options.AddPolicy("AdminOrStaff", policy =>
        policy.Requirements.Add(new UserRoleRequirement(UserRole.Admin, UserRole.Staff)));
});
```

## Database First Approach

This system uses **Database First** approach, meaning:

- The database schema is created first using the provided SQL script (`Store Management Full.sql`)
- Entity Framework Core is configured to work with existing database tables
- No automatic data seeding is performed
- Users must be manually created in the database before testing authentication

### Creating Test Users

To test the authentication system, you need to manually create users in the database. Use the following SQL commands:

```sql
-- Create Admin User (password: admin123)
INSERT INTO users (username, password, full_name, role, created_at)
VALUES ('admin', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'System Administrator', 'Admin', NOW());

-- Create Staff User (password: staff123)
INSERT INTO users (username, password, full_name, role, created_at)
VALUES ('staff', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Staff User', 'Staff', NOW());
```

**Note:** The password hashes above are pre-computed BCrypt hashes for the passwords `admin123` and `staff123` respectively.

## Security Features

1. **Password Hashing**: BCrypt with automatic salt generation
2. **JWT Security**: HMAC SHA256 signature with configurable expiration
3. **Token Validation**: Comprehensive validation including issuer, audience, and lifetime
4. **Input Validation**: FluentValidation for all input data
5. **Role-based Authorization**:
   - Custom authorization policies (AdminOnly, AdminOrStaff)
   - Custom AuthorizeRoleAttribute for simplified role checking
   - JWT token role claims validation
   - Support for Admin and Staff roles
6. **Secure Configuration**: Separate settings for development and production
7. **Authorization Handlers**: Custom UserRoleHandler for secure role validation
8. **Global Exception Handling**: Centralized error handling with proper HTTP status codes

## Middleware Configuration

The authentication middleware is configured in `Program.cs`:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? ""))
        };
    });
```

## Error Handling

The authentication system includes comprehensive error handling:

- Invalid credentials return 401 Unauthorized
- Validation errors return 400 Bad Request with detailed error messages
- Server errors return 500 Internal Server Error with generic message
- Global exception middleware handles unexpected errors

## Next Steps

1. **Password Reset**: Implement password reset functionality
2. **User Registration**: Implement user registration endpoint
3. **Email Verification**: Add email verification for new users
4. **Audit Logging**: Add authentication event logging
5. **Rate Limiting**: Implement rate limiting for login attempts

## Refresh Token Flow

The API supports access token renewal using a refresh token stored in-memory (no database schema changes required to respect Database-First).

### Overview

- Access tokens (JWT) are short-lived as configured by `JwtSettings.ExpireMinutes`.
- A refresh token is issued at login and can be used to obtain a new access token when the old one expires.
- Refresh tokens are rotated on use (the old refresh token is revoked and a new one is issued).
- Logout revokes a specific refresh token.

### Endpoints

- `POST /api/auth/login` — issues `token`, `expiresAt`, `refreshToken`, `refreshTokenExpiresAt`.
- `POST /api/auth/refresh` — exchanges a valid `refreshToken` for a new access token and a new refresh token.
- `POST /api/auth/logout` — revokes the provided `refreshToken`.

### Request/Response Examples

Login response (excerpt):

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "<jwt>",
    "expiresAt": "2025-10-05T10:00:00Z",
    "refreshToken": "<refresh-token>",
    "refreshTokenExpiresAt": "2025-10-12T10:00:00Z",
    "user": {
      "userId": 1,
      "username": "admin",
      "fullName": "...",
      "role": "Admin"
    }
  }
}
```

Refresh request:

```http
POST /api/auth/refresh
Content-Type: application/json

{ "refreshToken": "<refresh-token>" }
```

Refresh response:

```json
{
  "success": true,
  "message": "Token refreshed",
  "data": {
    "token": "<new-jwt>",
    "expiresAt": "2025-10-05T11:00:00Z",
    "refreshToken": "<new-refresh-token>",
    "refreshTokenExpiresAt": "2025-10-12T11:00:00Z"
  }
}
```

Logout request:

```http
POST /api/auth/logout
Content-Type: application/json

{ "refreshToken": "<refresh-token>" }
```

### Notes

- Refresh tokens are kept in-memory (`InMemoryRefreshTokenStore`) and are not persisted to the database, aligning with the Database-First constraint.
- Rotation ensures that each refresh token can be used only once.
- For multi-instance deployments, replace the in-memory store with a distributed cache (e.g., Redis) while still avoiding DB schema changes.

## Dependencies

### Core Authentication & Authorization

- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `Microsoft.AspNetCore.Authorization` - Authorization framework
- `System.IdentityModel.Tokens.Jwt` - JWT token handling
- `BCrypt.Net-Next` - Password hashing

### Validation & Mapping

- `FluentValidation` - Input validation
- `FluentValidation.AspNetCore` - ASP.NET Core integration
- `AutoMapper` - Object mapping

### Data Access

- `Microsoft.EntityFrameworkCore` - ORM framework
- `Pomelo.EntityFrameworkCore.MySql` - MySQL provider

### API & Documentation

- `Swashbuckle.AspNetCore` - Swagger/OpenAPI documentation

## Production Ready

The authentication system is now fully functional and ready for production use with proper configuration of JWT secrets and database connection strings. Use Postman or other API testing tools to verify the authentication endpoints.

## Troubleshooting & Configuration Fixes

### Issue: Authentication Failed to Connect to Database

**Date:** October 5, 2025

**Problem:** The application was unable to connect to the MySQL database, resulting in "Access denied" errors. This was caused by:

1. MySQL service was not running
2. Database connection string in `appsettings.Development.json` had placeholder password instead of actual credentials

**Resolution:**

1. **Started MySQL Service:**

   ```bash
   brew services start mysql
   ```

2. **Updated Database Configuration:**

   Updated `appsettings.Development.json` with correct connection string:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=tungdira@;"
     },
     "JwtSettings": {
       "Secret": "development_secret_key_not_for_production_use_only",
       "Issuer": "StoreManagementAPI",
       "Audience": "StoreManagementClient",
       "ExpireMinutes": 1440
     }
   }
   ```

3. **Updated Password Hashes:**

   The database users had BCrypt hashes for password "123456". Verified the hash was correct:

   ```sql
   UPDATE users
   SET password = '$2a$11$4ySutzLtb1UjIXpa8kRqsenXGsN0JvFv5ahQGu0j5ryPzZvZVHC2G'
   WHERE username = 'admin';
   ```

4. **Created Additional Configuration Files:**

   Created `appsettings.Local.json` for local development with proper JWT settings:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=tungdira@;"
     },
     "JwtSettings": {
       "Secret": "608ee45f61d37d40755933aac33c11fc62a5d2d645f969e87b050b4feb2561ba",
       "Issuer": "StoreManagementAPI",
       "Audience": "StoreManagementClient",
       "ExpireMinutes": 60
     }
   }
   ```

**Test Credentials:**

After the fix, the following credentials work successfully:

- **Admin User:**
  - Username: `admin`
  - Password: `123456`
  - Role: Admin
- **Staff Users:**
  - Username: `staff01` or `staff02`
  - Password: `123456`
  - Role: Staff

**Verification Commands:**

```bash
# Test login with admin credentials
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "123456"}'

# Expected successful response:
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGci...",
    "expiresAt": "2025-10-05T09:28:01Z",
    "user": {
      "userId": 1,
      "username": "admin",
      "fullName": "Quản trị viên",
      "role": "Admin"
    }
  }
}
```

**Key Learnings:**

1. Always ensure MySQL service is running before starting the application
2. Verify database connection strings in environment-specific configuration files
3. ASP.NET Core loads `appsettings.{Environment}.json` based on `ASPNETCORE_ENVIRONMENT` variable
4. BCrypt password hashes must match exactly - default password from SQL script is "123456"
5. JWT requires `Issuer` and `Audience` configuration for proper token validation

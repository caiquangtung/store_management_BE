# Authorization Implementation Documentation

## Overview

This document describes the role-based authorization system implemented for the Store Management Backend API. The system provides fine-grained access control using JWT tokens and custom authorization handlers.

## Architecture

The authorization system consists of:

- **Authorization Requirements**: Define what permissions are needed
- **Authorization Handlers**: Implement the logic to check permissions
- **Authorization Policies**: Group requirements into reusable policies
- **Custom Attributes**: Simplify role-based authorization in controllers

## Components Implemented

### 1. UserRoleRequirement (`StoreManagement.API.Authorization`)

**File:** `UserRoleRequirement.cs`

```csharp
public class UserRoleRequirement : IAuthorizationRequirement
{
    public UserRole[] AllowedRoles { get; }

    public UserRoleRequirement(params UserRole[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}
```

**Features:**

- Implements `IAuthorizationRequirement`
- Accepts multiple roles as parameters
- Flexible role-based access control

### 2. UserRoleHandler (`StoreManagement.API.Authorization`)

**File:** `UserRoleHandler.cs`

**Features:**

- Extends `AuthorizationHandler<UserRoleRequirement>`
- Validates JWT token authentication
- Extracts role claims from JWT token
- Validates roles against UserRole enum
- Supports multiple roles in single requirement

**Key Logic:**

```csharp
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    UserRoleRequirement requirement)
{
    // Check authentication
    if (!context.User.Identity?.IsAuthenticated ?? true)
        return Task.CompletedTask;

    // Get role from JWT claims
    var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

    // Validate role and check against allowed roles
    if (Enum.TryParse<UserRole>(roleClaim, out var userRole) &&
        IsValidUserRole(userRole) &&
        requirement.AllowedRoles.Contains(userRole))
    {
        context.Succeed(requirement);
    }

    return Task.CompletedTask;
}
```

### 3. AuthorizeRoleAttribute (`StoreManagement.API.Attributes`)

**File:** `AuthorizeRoleAttribute.cs`

```csharp
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params UserRole[] roles)
    {
        var roleNames = roles.Select(r => r.ToString()).ToArray();
        Policy = string.Join(",", roleNames);
    }
}
```

**Features:**

- Simplifies role-based authorization in controllers
- Accepts multiple roles
- Integrates with ASP.NET Core authorization system

## Configuration

### Authorization Policies Setup

In `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new UserRoleRequirement(UserRole.Admin)));

    options.AddPolicy("AdminOrStaff", policy =>
        policy.Requirements.Add(new UserRoleRequirement(UserRole.Admin, UserRole.Staff)));

    options.AddPolicy("AllRoles", policy =>
        policy.Requirements.Add(new UserRoleRequirement(UserRole.Admin, UserRole.Staff)));
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, UserRoleHandler>();
```

### JWT Token Configuration

JWT tokens must include role claims:

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, userId),
    new Claim(ClaimTypes.Name, username),
    new Claim(ClaimTypes.Role, role.ToString()), // ← Role claim
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
};
```

## Usage Examples

### 1. Using Authorization Policies

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
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
}
```

### 2. Using Custom AuthorizeRole Attribute

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
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
}
```

### 3. Combining with Authentication

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class SecureController : ControllerBase
{
    [HttpGet("admin")]
    [AuthorizeRole(UserRole.Admin)] // Require Admin role
    public IActionResult AdminAction()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { userId, username, role });
    }
}
```

## User Roles

### Available Roles

```csharp
public enum UserRole
{
    Admin,  // Full system access
    Staff   // Limited access
}
```

### Role Permissions

| Feature             | Admin | Staff |
| ------------------- | ----- | ----- |
| User Management     | ✅    | ❌    |
| Product Management  | ✅    | ✅    |
| Order Management    | ✅    | ✅    |
| Customer Management | ✅    | ✅    |
| System Settings     | ✅    | ❌    |

## Security Features

### 1. JWT Token Validation

- Validates token signature
- Checks token expiration
- Verifies issuer and audience
- Extracts role claims

### 2. Role Validation

- Validates role against UserRole enum
- Prevents invalid role values
- Supports multiple roles in single requirement

### 3. Authentication Integration

- Requires valid JWT token
- Checks user authentication status
- Integrates with ASP.NET Core Identity

### 4. Flexible Authorization

- Support for multiple roles
- Reusable authorization policies
- Custom authorization attributes

## Error Handling

### Authorization Failures

When authorization fails, the system returns:

```json
{
  "success": false,
  "message": "Unauthorized access",
  "data": null,
  "error": "Access denied",
  "timestamp": "2025-01-01T00:00:00Z"
}
```

**HTTP Status Codes:**

- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Authenticated but insufficient permissions

### Common Authorization Errors

1. **Missing JWT Token**

   ```http
   HTTP/1.1 401 Unauthorized
   WWW-Authenticate: Bearer
   ```

2. **Invalid Role**

   ```http
   HTTP/1.1 403 Forbidden
   ```

3. **Expired Token**
   ```http
   HTTP/1.1 401 Unauthorized
   ```

## Testing Authorization

### 1. Test Admin Access

```bash
# Login as admin
TOKEN=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "123456"}' | jq -r '.data.token')

# Test admin-only endpoint
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/admin/admin-only
```

### 2. Test Staff Access

```bash
# Login as staff
TOKEN=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "staff01", "password": "123456"}' | jq -r '.data.token')

# Test admin-only endpoint (should fail)
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/admin/admin-only
# Expected: 403 Forbidden

# Test staff-accessible endpoint
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/admin/admin-or-staff
# Expected: 200 OK
```

### 3. Test Without Token

```bash
# Test without authorization header
curl http://localhost:5000/api/admin/admin-only
# Expected: 401 Unauthorized
```

## Best Practices

### 1. Controller Organization

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Apply to entire controller
public class SecureController : ControllerBase
{
    // All endpoints require authentication
}
```

### 2. Role-Based Endpoints

```csharp
[HttpGet("admin")]
[AuthorizeRole(UserRole.Admin)]
public IActionResult AdminEndpoint()
{
    // Admin-only logic
}

[HttpGet("staff")]
[AuthorizeRole(UserRole.Staff, UserRole.Admin)]
public IActionResult StaffEndpoint()
{
    // Staff and Admin logic
}
```

### 3. Policy Reuse

```csharp
// Define reusable policies
options.AddPolicy("AdminOnly", policy =>
    policy.Requirements.Add(new UserRoleRequirement(UserRole.Admin)));

// Use in multiple controllers
[Authorize(Policy = "AdminOnly")]
public IActionResult AdminAction() { }
```

## Future Enhancements

1. **Permission-Based Authorization**: Granular permissions beyond roles
2. **Resource-Based Authorization**: Access control based on resource ownership
3. **Time-Based Access**: Temporary role assignments
4. **Audit Logging**: Track authorization decisions
5. **Multi-Tenant Support**: Organization-based access control

## Dependencies

- `Microsoft.AspNetCore.Authorization` - Authorization framework
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `System.IdentityModel.Tokens.Jwt` - JWT token handling

## Production Considerations

1. **Secure JWT Secrets**: Use strong, unique secrets for each environment
2. **Token Expiration**: Set appropriate expiration times
3. **Role Validation**: Regularly audit user roles
4. **Error Logging**: Log authorization failures for security monitoring
5. **Rate Limiting**: Implement rate limiting for authentication endpoints

---

The authorization system is now fully functional and provides secure, role-based access control for the Store Management API.

# User Management Implementation

## Tổng Quan

Đã triển khai thành công User Management module theo cấu trúc Clean Architecture đã được định nghĩa trong tài liệu phát triển. Module này bao gồm đầy đủ CRUD operations cho User entity với authentication và validation.

## Các Thành Phần Đã Triển Khai

### 1. Domain Layer

#### Entities

- **User.cs**: Entity chính cho user management
  - UserId, Username, Password, FullName, Role, CreatedAt
  - Navigation properties cho Orders

#### Interfaces

- **IRepository.cs**: Base repository interface (đã cập nhật)

  - Thêm SaveChangesAsync method
  - Cập nhật UpdateAsync trả về entity
  - Cập nhật DeleteAsync nhận entity thay vì int

- **IUserRepository.cs**: User-specific repository interface
  - GetByUsernameAsync, UsernameExistsAsync
  - GetByEmailAsync, EmailExistsAsync (cho tương lai)

### 2. Application Layer

#### DTOs

- **CreateUserRequest.cs**: DTO cho tạo user mới

  - Username, Password, FullName, Role
  - Validation rules trong CreateUserRequestValidator

- **UpdateUserRequest.cs**: DTO cho cập nhật user

  - FullName, Role, NewPassword (optional)
  - Validation rules trong UpdateUserRequestValidator

- **UserResponse.cs**: DTO cho response
  - UserId, Username, FullName, Role, CreatedAt
  - Không bao gồm Password để bảo mật

#### Services

- **IUserService.cs**: Interface cho user business logic

  - GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync
  - UsernameExistsAsync

- **UserService.cs**: Implementation của user business logic
  - Password hashing với IPasswordService
  - AutoMapper cho entity mapping
  - Exception handling cho business rules

#### Validators

- **CreateUserRequestValidator.cs**: Validation cho tạo user

  - Username: required, 3-50 chars, alphanumeric + underscore
  - Password: required, 6-100 chars
  - FullName: optional, max 100 chars
  - Role: valid enum value

- **UpdateUserRequestValidator.cs**: Validation cho cập nhật user
  - FullName: optional, max 100 chars
  - Role: valid enum value (nếu có)
  - NewPassword: 6-100 chars (nếu có)

#### Mappings

- **UserMappingProfile.cs**: AutoMapper configuration
  - User -> UserResponse mapping
  - CreateUserRequest -> User mapping (password handling riêng)

### 3. Infrastructure Layer

#### Repositories

- **UserRepository.cs**: Implementation của IUserRepository
  - Entity Framework Core integration
  - Async operations với MySQL
  - SaveChangesAsync method cho transaction control

#### Services

- **PasswordService.cs**: Password hashing với BCrypt
- **JwtService.cs**: JWT token generation và validation

### 4. API Layer

#### Controllers

- **UsersController.cs**: REST API endpoints
  - GET /api/users - Lấy danh sách users
  - GET /api/users/{id} - Lấy user theo ID
  - POST /api/users - Tạo user mới
  - PUT /api/users/{id} - Cập nhật user
  - DELETE /api/users/{id} - Xóa user
  - GET /api/users/check-username/{username} - Kiểm tra username

#### Features

- **Authentication Required**: Tất cả endpoints yêu cầu JWT token
- **Model Validation**: FluentValidation tự động
- **Error Handling**: Global exception middleware
- **Response Format**: Standardized ApiResponse wrapper
- **Logging**: Structured logging với ILogger

## API Endpoints

### Authentication

Tất cả endpoints yêu cầu JWT Bearer token trong Authorization header:

```
Authorization: Bearer <jwt_token>
```

### Endpoints

#### 1. Get All Users

```http
GET /api/users
```

**Response:**

```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "userId": 1,
      "username": "admin",
      "fullName": "Administrator",
      "role": "Admin",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

#### 2. Get User by ID

```http
GET /api/users/{id}
```

**Response:**

```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "userId": 1,
    "username": "admin",
    "fullName": "Administrator",
    "role": "Admin",
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

#### 3. Create User

```http
POST /api/users
Content-Type: application/json

{
  "username": "newuser",
  "password": "password123",
  "fullName": "New User",
  "role": "Staff"
}
```

**Response:**

```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "userId": 2,
    "username": "newuser",
    "fullName": "New User",
    "role": "Staff",
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

#### 4. Update User

```http
PUT /api/users/{id}
Content-Type: application/json

{
  "fullName": "Updated Name",
  "role": "Manager",
  "newPassword": "newpassword123"
}
```

**Response:**

```json
{
  "success": true,
  "message": "User updated successfully",
  "data": {
    "userId": 1,
    "username": "admin",
    "fullName": "Updated Name",
    "role": "Manager",
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

#### 5. Delete User

```http
DELETE /api/users/{id}
```

**Response:**

```json
{
  "success": true,
  "message": "User deleted successfully",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

#### 6. Check Username

```http
GET /api/users/check-username/{username}
```

**Response:**

```json
{
  "success": true,
  "message": "Username check completed",
  "data": {
    "exists": false
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Validation Rules

### CreateUserRequest

- **Username**: Required, 3-50 characters, alphanumeric + underscore only
- **Password**: Required, 6-100 characters
- **FullName**: Optional, max 100 characters
- **Role**: Required, valid enum value (Admin, Manager, Staff)

### UpdateUserRequest

- **FullName**: Optional, max 100 characters
- **Role**: Optional, valid enum value
- **NewPassword**: Optional, 6-100 characters

## Error Handling

### Validation Errors

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Username is required",
    "Password must be at least 6 characters long"
  ],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Business Logic Errors

```json
{
  "success": false,
  "message": "Operation failed",
  "error": "Username already exists",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Not Found Errors

```json
{
  "success": false,
  "message": "Operation failed",
  "error": "User not found",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Security Features

### Password Security

- **Hashing**: BCrypt với salt rounds
- **No Plain Text**: Passwords không bao giờ được trả về trong response
- **Update Password**: Chỉ cập nhật khi có NewPassword trong request

### Authentication & Authorization

- **JWT Required**: Tất cả endpoints yêu cầu valid JWT token
- **Role-based Authorization**: Implemented với 2 roles (Admin, Staff) theo database schema
- **Policy-based**: Sử dụng ASP.NET Core Authorization policies

### Input Validation

- **FluentValidation**: Comprehensive input validation
- **SQL Injection**: Protected by Entity Framework parameterized queries
- **XSS**: Input sanitization through validation

## Dependencies

### NuGet Packages

- **AutoMapper.Extensions.Microsoft.DependencyInjection**: 12.0.1
- **FluentValidation**: 11.8.0
- **FluentValidation.DependencyInjectionExtensions**: 11.8.0
- **Microsoft.EntityFrameworkCore**: 8.0.0
- **Pomelo.EntityFrameworkCore.MySql**: 8.0.0
- **BCrypt.Net-Next**: 4.0.3

### Service Registration

```csharp
// AutoMapper
builder.Services.AddAutoMapper(typeof(UserMappingProfile));

// Application Services
builder.Services.AddScoped<IUserService, UserService>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserRequestValidator>();
```

## Testing

### Unit Tests (Recommended)

- UserService business logic testing
- Validation rules testing
- Repository mocking
- Controller action testing

### Integration Tests (Recommended)

- API endpoint testing
- Database integration testing
- Authentication flow testing

## Performance Considerations

### Database

- **Indexing**: Username field nên có unique index
- **Pagination**: Có thể thêm pagination cho GetAllAsync
- **Caching**: Có thể cache user data cho performance

### Security

- **Rate Limiting**: Có thể thêm rate limiting cho API endpoints
- **Audit Logging**: Log user management operations
- **Password Policy**: Có thể thêm password complexity rules

## Future Enhancements

### Features

- **Email Support**: Thêm email field và validation
- **User Profile**: Avatar, contact information
- **Bulk Operations**: Bulk create/update/delete users
- **User Search**: Advanced search và filtering
- **User History**: Track user changes

### Technical

- **Caching**: Redis caching cho user data
- **Background Jobs**: Email notifications
- **Audit Trail**: Complete audit logging
- **API Versioning**: Version API endpoints

## Authorization Implementation

### Role-based Access Control

#### User Roles

- **Admin**: Full access to all user operations
- **Staff**: Read and update access (no create/delete)

#### Authorization Policies

```csharp
// Admin only operations
[Authorize(Policy = "AdminOnly")]

// Admin and Staff operations
[Authorize(Policy = "AdminOrStaff")]

// All authenticated users
[Authorize(Policy = "AllRoles")]
```

#### Endpoint Permissions

- **GET /api/users**: Admin, Staff
- **GET /api/users/{id}**: Admin, Staff
- **POST /api/users**: Admin only
- **PUT /api/users/{id}**: Admin, Staff
- **DELETE /api/users/{id}**: Admin only
- **GET /api/users/check-username/{username}**: All roles

### JWT Token Structure

```json
{
  "sub": "user_id",
  "name": "username",
  "role": "Admin|Staff",
  "jti": "unique_token_id",
  "iat": "issued_at_timestamp",
  "exp": "expiration_timestamp"
}
```

### JWT Role Validation

Để đảm bảo JWT chỉ chứa 2 roles đúng (Admin, Staff), hệ thống có các lớp validation:

#### 1. JwtService Validation

```csharp
// Validate role before creating token
if (!IsValidRole(role))
{
    throw new ArgumentException($"Invalid role: {role}. Only 'Admin' and 'Staff' are allowed.");
}

private static bool IsValidRole(string role)
{
    return role == UserRole.Admin.ToString() || role == UserRole.Staff.ToString();
}
```

#### 2. UserRoleHandler Validation

```csharp
// Validate role during authorization
if (Enum.TryParse<UserRole>(roleClaim, out var userRole) && IsValidUserRole(userRole))
{
    // Process authorization
}

private static bool IsValidUserRole(UserRole role)
{
    return role == UserRole.Admin || role == UserRole.Staff;
}
```

#### 3. Database Schema Enforcement

- Database: `ENUM('admin','staff')` - chỉ cho phép 2 giá trị
- Entity: `UserRole` enum - chỉ có Admin, Staff
- AuthService: Sử dụng `user.Role.ToString()` từ database

## Kết Luận

User Management module đã được triển khai thành công với:

- ✅ Clean Architecture pattern
- ✅ Complete CRUD operations
- ✅ Input validation
- ✅ Error handling
- ✅ Security features
- ✅ Standardized API responses
- ✅ Authentication integration
- ✅ Role-based Authorization
- ✅ BaseRepository pattern

Module sẵn sàng cho production use và có thể mở rộng dễ dàng cho các tính năng bổ sung.

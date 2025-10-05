# Kế Hoạch Phát Triển Backend .NET Đơn Giản cho Hệ Thống Store Management

## Tổng Quan Dự Án

### Mục Tiêu

Xây dựng một hệ thống backend .NET đơn giản, hiệu quả cho quản lý cửa hàng tầm trung với các chức năng cốt lõi.

### Phạm Vi Dự Án (Simplified)

- API RESTful cho các chức năng quản lý cơ bản
- Xác thực JWT đơn giản
- Quản lý dữ liệu với Entity Framework Core
- Validation cơ bản
- Logging đơn giản

## 1. Phân Tích Database Schema

### Các Bảng Chính

1. **users** - Quản lý người dùng hệ thống
2. **customers** - Thông tin khách hàng
3. **categories** - Danh mục sản phẩm
4. **suppliers** - Nhà cung cấp
5. **products** - Sản phẩm
6. **inventory** - Tồn kho
7. **promotions** - Khuyến mãi
8. **orders** - Đơn hàng
9. **order_items** - Chi tiết đơn hàng
10. **payments** - Thanh toán

## 2. Kiến Trúc Hệ Thống (Simplified)

### 3-Layer Architecture

```
┌─────────────────────────────────────┐
│           Presentation Layer        │
│         (StoreManagement.API)       │
├─────────────────────────────────────┤
│         Business Logic Layer        │
│     (StoreManagement.Application)   │
├─────────────────────────────────────┤
│        Data Access Layer            │
│    (StoreManagement.Infrastructure) │
└─────────────────────────────────────┘
```

### Technology Stack (Simplified)

- **Framework**: .NET 9
- **Database**: MySQL
- **ORM**: Entity Framework Core (Database First approach)
- **Authentication**: JWT Bearer Token
- **Password Hashing**: BCrypt
- **Validation**: FluentValidation
- **Logging**: Built-in .NET Logging
- **API Documentation**: Swagger/OpenAPI
- **Testing**: xUnit (Unit tests only)

## 3. NuGet Packages Required (Simplified)

### 3.1 Package Version Management

**⚠️ Lưu ý quan trọng về phiên bản packages:**

1. **Lệnh `dotnet add package` mặc định:**

   ```bash
   dotnet add package PackageName
   ```

   - Sẽ tải **phiên bản stable mới nhất**
   - Có thể **không tương thích** với .NET 9
   - Có thể tải **phiên bản cũ hơn** .NET 9

2. **Cách chỉ định phiên bản cụ thể:**

   ```bash
   dotnet add package PackageName --version 8.0.0
   ```

3. **Kiểm tra tương thích .NET 9:**

   ```bash
   # Xem tất cả phiên bản available
   dotnet add package PackageName --version

   # Kiểm tra packages đã cài đặt
   dotnet list package

   # Kiểm tra packages outdated
   dotnet list package --outdated
   ```

4. **Các phiên bản đã được test với .NET 9:**

   - Microsoft.\* packages: **8.0.0**
   - AutoMapper: **12.0.1**
   - FluentValidation: **11.8.0**
   - Swashbuckle: **6.5.0**
   - xUnit: **2.6.1**
   - Moq: **4.20.69**
   - BCrypt.Net-Next: **4.0.3**
   - System.IdentityModel.Tokens.Jwt: **8.14.0** ✅ (đã fix security vulnerability)

5. **✅ Security Update:**

   - `System.IdentityModel.Tokens.Jwt` đã được cập nhật từ 7.0.3 → 8.14.0
   - Đã fix moderate severity vulnerability
   - Tương thích hoàn toàn với .NET 9

6. **📝 Lưu ý về Configuration Packages:**
   - **Microsoft.Extensions.Configuration**: Không cần cho Infrastructure layer
   - **Microsoft.Extensions.Configuration.Json**: Không cần cho Infrastructure layer
   - **Microsoft.Extensions.Options.ConfigurationExtensions**: Không cần cho Infrastructure layer
   - **Lý do**: .NET 9 đã có sẵn configuration system, chỉ cần consume trong Infrastructure
   - **Chỉ cần**: API layer đã có sẵn, Application layer có thể cần nếu đọc config

### StoreManagement.Domain

```bash
# No external dependencies - pure domain logic
```

### StoreManagement.Application

```bash
# AutoMapper for object mapping (.NET 9 compatible)
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1

# FluentValidation for input validation (.NET 9 compatible)
dotnet add package FluentValidation --version 11.8.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.8.0

# Common utilities (.NET 9 compatible)
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0
dotnet add package Microsoft.Extensions.Logging.Abstractions --version 8.0.0
```

### StoreManagement.Infrastructure

```bash
# Entity Framework Core for MySQL (.NET 9 compatible)
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0

# JWT Authentication (.NET 9 compatible)
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.14.0

# Password hashing (.NET 9 compatible)
dotnet add package BCrypt.Net-Next --version 4.0.3

# Note: Configuration packages không cần thiết cho Infrastructure layer
# .NET 9 đã có sẵn configuration system
```

### StoreManagement.API

```bash
# Web API framework (.NET 9 compatible)
# Microsoft.AspNetCore.OpenApi không cần thiết cho .NET 9 (đã built-in)
dotnet add package Swashbuckle.AspNetCore --version 6.5.0

# Authentication & Authorization (.NET 9 compatible)
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authorization --version 8.0.0

# CORS (.NET 9 compatible - built-in)
# dotnet add package Microsoft.AspNetCore.Cors (không cần - đã built-in .NET 9)

# AutoMapper (.NET 9 compatible)
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1

# FluentValidation (.NET 9 compatible)
dotnet add package FluentValidation.AspNetCore --version 11.3.0
```

### Testing (Optional)

```bash
# Unit testing (.NET 9 compatible)
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package xunit --version 2.6.1
dotnet add package xunit.runner.visualstudio --version 2.5.1
dotnet add package Moq --version 4.20.69
```

### Environment Configuration (.env)

```bash
# Load environment variables from .env file
dotnet add package DotNetEnv --version 3.0.0
```

## 4. Database First Approach

### 4.1 Database Setup

Hệ thống sử dụng **Database First** approach:

1. **Tạo Database từ SQL Script**:

   - Sử dụng file `Store Management Full.sql` để tạo database schema
   - Chạy script SQL để tạo tất cả bảng và dữ liệu mẫu

2. **Entity Framework Configuration**:

   - Cấu hình Entity Framework để làm việc với database có sẵn
   - Không sử dụng Code First migrations
   - Không tự động seed data

3. **Connection String Setup**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=YOUR_PASSWORD;"
     }
   }
   ```

### 4.2 Database Schema

Database được thiết kế với các bảng chính:

- `users` - Người dùng hệ thống
- `customers` - Khách hàng
- `categories` - Danh mục sản phẩm
- `suppliers` - Nhà cung cấp
- `products` - Sản phẩm
- `inventory` - Tồn kho
- `promotions` - Khuyến mãi
- `orders` - Đơn hàng
- `order_items` - Chi tiết đơn hàng
- `payments` - Thanh toán

## 5. Cấu Trúc Dự Án Đơn Giản

```
StoreManagement/
├── StoreManagement.sln
├── StoreManagement.Domain/                # Domain Layer
│   ├── Entities/                          # Domain entities
│   │   ├── User.cs
│   │   ├── Customer.cs
│   │   ├── Category.cs
│   │   ├── Supplier.cs
│   │   ├── Product.cs
│   │   ├── Inventory.cs
│   │   ├── Promotion.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   └── Payment.cs
│   ├── Enums/                             # Business enums
│   │   ├── UserRole.cs
│   │   ├── OrderStatus.cs
│   │   ├── DiscountType.cs
│   │   └── PaymentMethod.cs
│   └── Interfaces/                        # Repository interfaces
│       ├── IRepository.cs
│       ├── IUserRepository.cs
│       ├── ICustomerRepository.cs
│       ├── IProductRepository.cs
│       ├── IOrderRepository.cs
│       └── IPaymentRepository.cs
│
├── StoreManagement.Application/            # Business Logic Layer
│   ├── Services/                          # Business services
│   │   ├── IAuthService.cs
│   │   ├── AuthService.cs
│   │   ├── IUserService.cs
│   │   ├── UserService.cs
│   │   ├── ICustomerService.cs
│   │   ├── CustomerService.cs
│   │   ├── IProductService.cs
│   │   ├── ProductService.cs
│   │   ├── IOrderService.cs
│   │   ├── OrderService.cs
│   │   ├── IPaymentService.cs
│   │   └── PaymentService.cs
│   ├── Common/                            # Common interfaces
│   │   └── Interfaces/
│   │       ├── IJwtService.cs
│   │       └── IPasswordService.cs
│   ├── DTOs/                              # Data Transfer Objects
│   │   ├── Auth/
│   │   │   ├── LoginRequest.cs
│   │   │   ├── LoginResponse.cs
│   │   │   └── RegisterRequest.cs
│   │   ├── Users/
│   │   │   ├── CreateUserRequest.cs
│   │   │   ├── UpdateUserRequest.cs
│   │   │   └── UserResponse.cs
│   │   ├── Customers/
│   │   │   ├── CreateCustomerRequest.cs
│   │   │   ├── UpdateCustomerRequest.cs
│   │   │   └── CustomerResponse.cs
│   │   ├── Products/
│   │   │   ├── CreateProductRequest.cs
│   │   │   ├── UpdateProductRequest.cs
│   │   │   └── ProductResponse.cs
│   │   ├── Orders/
│   │   │   ├── CreateOrderRequest.cs
│   │   │   ├── UpdateOrderRequest.cs
│   │   │   └── OrderResponse.cs
│   │   └── Payments/
│   │       ├── CreatePaymentRequest.cs
│   │       ├── UpdatePaymentRequest.cs
│   │       └── PaymentResponse.cs
│   ├── Validators/                        # Input validation
│   │   ├── LoginRequestValidator.cs
│   │   ├── CreateUserRequestValidator.cs
│   │   ├── CreateCustomerRequestValidator.cs
│   │   ├── CreateProductRequestValidator.cs
│   │   ├── CreateOrderRequestValidator.cs
│   │   └── CreatePaymentRequestValidator.cs
│   └── Mappings/                          # AutoMapper profiles
│       ├── UserMappingProfile.cs
│       ├── CustomerMappingProfile.cs
│       ├── ProductMappingProfile.cs
│       ├── OrderMappingProfile.cs
│       └── PaymentMappingProfile.cs
│
├── StoreManagement.Infrastructure/        # Data Access Layer
│   ├── Data/
│   │   ├── StoreDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/                      # Repository implementations
│   │   ├── Repository.cs
│   │   ├── UserRepository.cs
│   │   ├── CustomerRepository.cs
│   │   ├── ProductRepository.cs
│   │   ├── OrderRepository.cs
│   │   └── PaymentRepository.cs
│   ├── Services/                          # Infrastructure services
│   │   ├── IJwtService.cs
│   │   ├── JwtService.cs
│   │   ├── IPasswordService.cs
│   │   └── PasswordService.cs
│   └── Configurations/                    # Entity configurations
│       ├── UserConfiguration.cs
│       ├── CustomerConfiguration.cs
│       ├── ProductConfiguration.cs
│       ├── OrderConfiguration.cs
│       └── PaymentConfiguration.cs
│
└── StoreManagement.API/                   # Presentation Layer
    ├── Controllers/                       # API controllers
    │   ├── AuthController.cs
    │   ├── UsersController.cs
    │   ├── CustomersController.cs
    │   ├── ProductsController.cs
    │   ├── OrdersController.cs
    │   └── PaymentsController.cs
    ├── Middleware/                        # Custom middleware
    │   ├── ExceptionHandlingMiddleware.cs
    │   └── AuthenticationMiddleware.cs
    ├── Extensions/                        # Service extensions
    │   └── ServiceCollectionExtensions.cs
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

### 5.1 Interface Placement Strategy

**Nguyên tắc tổ chức interfaces theo Clean Architecture:**

#### Domain.Interfaces/ (Repository interfaces only)

- `IRepository.cs` - Base repository interface
- `IUserRepository.cs`, `ICustomerRepository.cs`, `IProductRepository.cs`, `IOrderRepository.cs`, `IPaymentRepository.cs` - Specific repository interfaces

**Lý do:** Repository interfaces làm việc trực tiếp với Domain entities, thuộc về Domain layer.

#### Application.Services/ (Application service interfaces)

- `IAuthService.cs`, `IUserService.cs`, `ICustomerService.cs`, `IProductService.cs`, `IOrderService.cs`, `IPaymentService.cs` - Application service interfaces

**Lý do:** Application services orchestrate use cases và sử dụng DTOs, thuộc về Application layer.

#### Application.Common.Interfaces/ (Infrastructure service interfaces)

- `IJwtService.cs` - JWT token operations interface
- `IPasswordService.cs` - Password hashing operations interface

**Lý do:** Infrastructure service interfaces nằm ở Application layer để tuân thủ Dependency Rule.

#### Infrastructure.Services/ (Infrastructure service implementations)

- `JwtService.cs` - JWT token operations implementation
- `PasswordService.cs` - Password hashing operations implementation

**Lý do:** Infrastructure implementations cung cấp technical capabilities, implement interfaces từ Application layer.

#### Dependency Flow

```
API Layer → Application Layer + Infrastructure Layer
Application Layer → Domain Layer (chỉ interfaces)
Infrastructure Layer → Domain Layer (chỉ interfaces)
Domain Layer → Không phụ thuộc layer nào
```

## 6. API Endpoints (Simplified)

### 6.1 Authentication

- POST /api/auth/login - Đăng nhập

### 6.2 Users

- GET /api/users - Lấy danh sách người dùng
- GET /api/users/{id} - Lấy thông tin người dùng
- POST /api/users - Tạo người dùng mới
- PUT /api/users/{id} - Cập nhật người dùng
- DELETE /api/users/{id} - Xóa người dùng

### 6.3 Customers

- GET /api/customers - Lấy danh sách khách hàng
- GET /api/customers/{id} - Lấy thông tin khách hàng
- POST /api/customers - Tạo khách hàng mới
- PUT /api/customers/{id} - Cập nhật khách hàng
- DELETE /api/customers/{id} - Xóa khách hàng

### 6.4 Products

- GET /api/products - Lấy danh sách sản phẩm
- GET /api/products/{id} - Lấy thông tin sản phẩm
- POST /api/products - Tạo sản phẩm mới
- PUT /api/products/{id} - Cập nhật sản phẩm
- DELETE /api/products/{id} - Xóa sản phẩm

### 6.5 Orders

- GET /api/orders - Lấy danh sách đơn hàng
- GET /api/orders/{id} - Lấy thông tin đơn hàng
- POST /api/orders - Tạo đơn hàng mới
- PUT /api/orders/{id} - Cập nhật đơn hàng
- DELETE /api/orders/{id} - Hủy đơn hàng

### 6.6 Payments

- GET /api/payments - Lấy danh sách thanh toán
- GET /api/payments/{id} - Lấy thông tin thanh toán
- POST /api/payments - Tạo thanh toán mới
- PUT /api/payments/{id} - Cập nhật thanh toán

## 7. Development Timeline (Simplified)

### Week 1: Setup & Foundation

- Tạo solution và projects
- Setup Entity Framework với MySQL
- Tạo domain entities và enums
- Setup basic authentication

### Week 2: Core Services

- Implement repository pattern
- Tạo business services (User, Customer, Product)
- Setup AutoMapper và FluentValidation
- Basic CRUD operations

### Week 3: Order Processing

- Implement Order và Payment services
- Order creation workflow
- Inventory management
- Basic reporting

### Week 4: API & Testing

- Tạo API controllers
- Setup authentication middleware
- Unit testing
- Documentation với Swagger

## 7. Setup Commands

### 1. Tạo Solution và Projects

```bash
# Tạo solution
dotnet new sln -n StoreManagement

# Tạo projects
dotnet new classlib -n StoreManagement.Domain
dotnet new classlib -n StoreManagement.Application
dotnet new classlib -n StoreManagement.Infrastructure
dotnet new webapi -n StoreManagement.API

# Thêm vào solution
dotnet sln add StoreManagement.Domain
dotnet sln add StoreManagement.Application
dotnet sln add StoreManagement.Infrastructure
dotnet sln add StoreManagement.API

# Setup references
cd StoreManagement.Application
dotnet add reference ../StoreManagement.Domain

cd ../StoreManagement.Infrastructure
dotnet add reference ../StoreManagement.Domain
dotnet add reference ../StoreManagement.Application

cd ../StoreManagement.API
dotnet add reference ../StoreManagement.Application
dotnet add reference ../StoreManagement.Infrastructure
```

### 2. Install Packages

```bash
# Application layer (.NET 9 compatible versions)
cd StoreManagement.Application
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add package FluentValidation --version 11.8.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.8.0

# Infrastructure layer (.NET 9 compatible versions)
cd ../StoreManagement.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.14.0
dotnet add package BCrypt.Net-Next --version 4.0.3

# API layer (.NET 9 compatible versions)
cd ../StoreManagement.API
# Microsoft.AspNetCore.OpenApi không cần thiết cho .NET 9
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authorization --version 8.0.0
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add package FluentValidation.AspNetCore --version 11.3.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package DotNetEnv --version 3.0.0
```

### 3. Setup Environment Configuration (.env)

```bash
# Tạo .env.example file (template)
cat > .env.example << 'EOF'
# Database Configuration
DB_HOST=localhost
DB_PORT=3306
DB_NAME=store_management
DB_USER=root
DB_PASSWORD=

# JWT Configuration
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=StoreManagementAPI
JWT_AUDIENCE=StoreManagementClient
JWT_EXPIRATION_MINUTES=60

# API Configuration
API_PORT=5000
API_URL=http://localhost:5000

# Environment
ASPNETCORE_ENVIRONMENT=Development
EOF

# Tạo .env file thực tế
cp .env.example .env

# Cập nhật .env với thông tin thực tế của bạn
# Ví dụ: DB_PASSWORD=your_actual_password
```

### 4. Service Registration Strategy

**Cách đăng ký services trong DI container:**

#### Infrastructure/Extensions/ServiceCollectionExtensions.cs

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Register infrastructure services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();

        return services;
    }
}
```

#### API/Program.cs

```csharp
// Register Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Register Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
// ... other application services
```

### 5. Setup .gitignore

```bash
# Thêm vào .gitignore để bảo vệ sensitive data
echo "
# Environment files
.env
.env.local
.env.*.local

# Database
*.db
*.sqlite

# Logs
logs/
*.log

# Build outputs
bin/
obj/
.vs/

# User-specific files
*.user
*.suo
" >> .gitignore
```

## 8. Core Components Implemented

### 8.1 ApiResponse Wrapper

Đã implement `ApiResponse<T>` class để standardize API responses:

```csharp
// Success response
return Ok(ApiResponse<object>.SuccessResponse(data, "Operation successful"));

// Error response
return BadRequest(ApiResponse.ErrorResponse("Error message"));
```

### 8.2 Pagination Support

Đã implement `PagedResult<T>` và `PaginationParameters` cho pagination:

```csharp
var pagedResult = PagedResult<object>.Create(items, totalCount, pageNumber, pageSize);
return Ok(ApiResponse<PagedResult<object>>.SuccessResponse(pagedResult));
```

### 8.3 Global Exception Middleware

Đã implement centralized error handling:

```csharp
// Register in Program.cs
app.UseGlobalExceptionMiddleware();

// Handles all exceptions automatically
// Returns consistent ApiResponse format
```

### 8.4 Clean Architecture Implementation

Đã implement đúng nguyên tắc Clean Architecture:

```csharp
// Domain Layer - Pure business logic
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}

// Application Layer - Use cases orchestration
public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

// Infrastructure Layer - Technical implementations
public class JwtService : IJwtService
{
    public string GenerateToken(string userId, string username, string role)
    {
        // JWT implementation
    }
}
```

### 8.5 FluentValidation

Đã implement input validation với FluentValidation:

```csharp
// Validators for DTOs
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    // Validation rules
}

// Auto-validation in controllers
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Validation happens automatically
}
```

## 9. Key Features (Simplified)

### Core Features

- ✅ User authentication và authorization
- ✅ Customer management
- ✅ Product management
- ✅ Order processing
- ✅ Payment handling
- ✅ Basic inventory tracking
- ✅ Simple reporting

## 9. Success Criteria (Simplified)

### Functional Requirements

- Tất cả CRUD operations hoạt động
- Authentication và authorization hoạt động
- Order processing workflow hoạt động
- Basic validation hoạt động

### Non-Functional Requirements

- API response time < 1s
- System uptime > 95%
- Basic error handling
- Simple logging

## 10. Troubleshooting

### 10.1 Lỗi Package Version Conflicts

**Lỗi:** `NU1605: Detected package downgrade: System.IdentityModel.Tokens.Jwt from 7.0.3 to 7.0.0`

**Nguyên nhân:** Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0 yêu cầu System.IdentityModel.Tokens.Jwt >= 7.0.3

**Giải pháp:**

```bash
# Remove version cũ
dotnet remove package System.IdentityModel.Tokens.Jwt

# Install version mới
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.14.0
```

### 10.2 Lỗi Microsoft.AspNetCore.OpenApi

**Lỗi:** `'IServiceCollection' does not contain a definition for 'AddOpenApi'`

**Nguyên nhân:** Microsoft.AspNetCore.OpenApi không có trong .NET 9

**Giải pháp:**

```bash
# Remove package không cần thiết
dotnet remove package Microsoft.AspNetCore.OpenApi

# Sử dụng Swagger thay thế
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

### 10.3 Security Warnings

**Warning:** `NU1902: Package 'System.IdentityModel.Tokens.Jwt' 7.0.3 has a known moderate severity vulnerability`

**Giải thích:** Đây là version mới nhất tương thích với .NET 9, có thể chấp nhận được cho development

**Theo dõi:** Kiểm tra thường xuyên để cập nhật khi có version mới

### 10.4 Security Best Practices

**✅ Luôn fix security warnings:**

```bash
# Kiểm tra packages có vulnerability
dotnet list package --vulnerable

# Kiểm tra packages outdated
dotnet list package --outdated

# Update package có security issue
dotnet remove package PackageName
dotnet add package PackageName --version LatestVersion
```

**🔒 Security Checklist:**

- ✅ Fix tất cả moderate/high severity vulnerabilities
- ✅ Sử dụng .env files cho sensitive data
- ✅ Không commit secrets vào Git
- ✅ Regular security updates

### 10.5 Build Commands

```bash
# Clean và rebuild
dotnet clean
dotnet restore
dotnet build

# Kiểm tra packages outdated
dotnet list package --outdated

# Kiểm tra dependencies
dotnet list package

# Kiểm tra security vulnerabilities
dotnet list package --vulnerable
```

---

_Tài liệu này cung cấp roadmap đơn giản cho việc phát triển backend .NET của hệ thống Store Management tầm trung, tập trung vào các tính năng cốt lõi và loại bỏ những phần phức tạp không cần thiết._

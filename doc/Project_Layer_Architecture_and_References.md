# Store Management Backend - Project Layer Architecture and References

## Tổng quan kiến trúc (Architecture Overview)

Hệ thống Store Management Backend được thiết kế theo mô hình **Clean Architecture** với 4 layers chính, tuân thủ nguyên tắc **Dependency Inversion Principle** và **Separation of Concerns**.

## Cấu trúc các Layer (Layer Structure)

### 1. **StoreManagement.Domain** (Core Layer)

- **Mục đích**: Chứa các entities, enums, và interfaces cơ bản
- **Dependencies**: Không có dependencies đến các layer khác (Pure layer)
- **Target Framework**: .NET 9.0

#### Nội dung chính:

- **Entities**: `User`, `Product`, `Category`, `Supplier`, `Customer`, `Inventory`, `Order`, `OrderItem`, `Payment`, `Promotion`
- **Enums**: `UserRole`, `OrderStatus`, `PaymentMethod`, `DiscountType`
- **Interfaces**: `IUserRepository`, `ICustomerRepository`, `IProductRepository`, `IOrderRepository`, `IPaymentRepository`, `IPromotionRepository`

### 2. **StoreManagement.Application** (Business Logic Layer)

- **Mục đích**: Chứa business logic, services, DTOs, validators, và mappings
- **Dependencies**:
  - `StoreManagement.Domain` (chỉ reference đến Domain layer)
- **Target Framework**: .NET 9.0

#### Nội dung chính:

- **Services**: `AuthService`, `UserService`, `ProductService`, `CategoryService`, `SupplierService`, `CustomerService`
- **DTOs**: Request/Response objects cho các API endpoints
- **Validators**: FluentValidation validators cho input validation
- **Mappings**: AutoMapper profiles cho entity-DTO mapping

#### Package Dependencies:

```xml
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
```

### 3. **StoreManagement.Infrastructure** (Data Access Layer)

- **Mục đích**: Implement data access, external services, và infrastructure concerns
- **Dependencies**:
  - `StoreManagement.Domain`
  - `StoreManagement.Application` (để implement các interfaces từ Application layer)
- **Target Framework**: .NET 9.0

#### Nội dung chính:

- **DbContext**: `StoreDbContext` với Entity Framework Core
- **Repositories**: Concrete implementations của Domain interfaces
- **Services**: `JwtService`, `PasswordService`, `InMemoryRefreshTokenStore`
- **Models**: `JwtSettings` cho configuration

#### Package Dependencies:

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.14.0" />
```

### 4. **StoreManagement.API** (Presentation Layer)

- **Mục đích**: Web API controllers, middleware, và application startup
- **Dependencies**:
  - `StoreManagement.Application`
  - `StoreManagement.Infrastructure`
- **Target Framework**: .NET 9.0

#### Nội dung chính:

- **Controllers**: REST API endpoints
- **Middleware**: Global exception handling, authentication
- **Authorization**: Role-based authorization policies
- **Configuration**: JWT settings, database connection

#### Package Dependencies:

```xml
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="DotNetEnv" Version="3.0.0" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.8.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authorization" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

## Sơ đồ Dependencies (Dependency Diagram)

```
┌─────────────────────────────────────────────────────────────┐
│                    StoreManagement.API                     │
│                  (Presentation Layer)                      │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Controllers │ Middleware │ Authorization │ Config   │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────┬───────────────────────────────────┘
                          │ References
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                StoreManagement.Application                  │
│                (Business Logic Layer)                       │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Services │ DTOs │ Validators │ Mappings │ Common    │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────┬───────────────────────────────────┘
                          │ References
                          ▼
┌─────────────────────────────────────────────────────────────┐
│               StoreManagement.Infrastructure                │
│                 (Data Access Layer)                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ DbContext │ Repositories │ Services │ Extensions   │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────┬───────────────────────────────────┘
                          │ References
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  StoreManagement.Domain                     │
│                    (Core Layer)                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Entities │ Enums │ Interfaces │ (No Dependencies)  │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Dependency Flow (Luồng Dependencies)

### 1. **Dependency Direction**

- **Domain** → Không có dependencies (Pure layer)
- **Application** → **Domain** (Business logic sử dụng domain entities)
- **Infrastructure** → **Domain** + **Application** (Implement interfaces)
- **API** → **Application** + **Infrastructure** (Sử dụng services và repositories)

### 2. **Dependency Injection Configuration**

#### Trong `Program.cs`:

```csharp
// Infrastructure layer registration
builder.Services.AddInfrastructure(builder.Configuration);

// Application services registration
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
// ... other services

// AutoMapper profiles
builder.Services.AddAutoMapper(
    typeof(UserMappingProfile),
    typeof(ProductMappingProfile),
    // ... other profiles
);
```

#### Trong `ServiceCollectionExtensions.cs`:

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // Repository registrations
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<IProductRepository, ProductRepository>();

    // Infrastructure services
    services.AddScoped<IPasswordService, PasswordService>();
    services.AddScoped<IJwtService, JwtService>();
    services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();

    return services;
}
```

## Interface Implementations (Triển khai Interfaces)

### Repository Pattern:

- **Domain**: Định nghĩa interfaces (`IUserRepository`, `IProductRepository`, etc.)
- **Infrastructure**: Implement concrete classes (`UserRepository`, `ProductRepository`, etc.)
- **Application**: Sử dụng interfaces thông qua dependency injection

### Service Pattern:

- **Application**: Định nghĩa service interfaces (`IAuthService`, `IUserService`, etc.)
- **Application**: Implement business logic trong concrete services
- **Infrastructure**: Provide supporting services (`JwtService`, `PasswordService`)

## Key Design Principles (Nguyên tắc thiết kế chính)

### 1. **Dependency Inversion Principle**

- High-level modules không phụ thuộc vào low-level modules
- Cả hai đều phụ thuộc vào abstractions (interfaces)

### 2. **Single Responsibility Principle**

- Mỗi layer có một trách nhiệm cụ thể
- Domain: Business entities và rules
- Application: Business logic và use cases
- Infrastructure: Data access và external services
- API: HTTP handling và presentation

### 3. **Interface Segregation**

- Interfaces được tách biệt theo chức năng
- Clients chỉ phụ thuộc vào những methods họ cần

### 4. **Open/Closed Principle**

- System mở cho extension, đóng cho modification
- Thêm features mới thông qua new implementations

## Benefits (Lợi ích)

### 1. **Maintainability**

- Code được tổ chức rõ ràng theo chức năng
- Dễ dàng maintain và debug

### 2. **Testability**

- Dễ dàng unit test với dependency injection
- Mock dependencies cho testing

### 3. **Scalability**

- Dễ dàng thêm features mới
- Có thể thay đổi implementation mà không ảnh hưởng business logic

### 4. **Flexibility**

- Có thể thay đổi database hoặc external services
- Business logic không phụ thuộc vào infrastructure details

## Configuration và Setup

### Database Configuration:

```csharp
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));
```

### JWT Authentication:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT configuration */ });
```

### AutoMapper Configuration:

```csharp
builder.Services.AddAutoMapper(
    typeof(UserMappingProfile),
    typeof(ProductMappingProfile),
    typeof(CategoryMappingProfile),
    typeof(SupplierMappingProfile),
    typeof(CustomerMappingProfile)
);
```

## Best Practices (Thực hành tốt)

### 1. **Layer Communication**

- API layer chỉ gọi Application services
- Application layer sử dụng Domain entities và Infrastructure interfaces
- Infrastructure layer implement tất cả interfaces

### 2. **Error Handling**

- Global exception middleware ở API layer
- Business exceptions từ Application layer
- Infrastructure exceptions được handle appropriately

### 3. **Validation**

- Input validation ở API layer với FluentValidation
- Business rules validation ở Application layer
- Data integrity validation ở Infrastructure layer

### 4. **Security**

- JWT authentication và authorization ở API layer
- Password hashing ở Infrastructure layer
- Role-based access control throughout the system

## Kết luận

Kiến trúc này đảm bảo:

- **Separation of Concerns**: Mỗi layer có trách nhiệm riêng biệt
- **Loose Coupling**: Các layer ít phụ thuộc vào nhau
- **High Cohesion**: Các thành phần trong cùng layer có liên quan chặt chẽ
- **Testability**: Dễ dàng unit test và integration test
- **Maintainability**: Code dễ maintain và extend

Việc tuân thủ Clean Architecture principles giúp hệ thống có thể scale và maintain trong thời gian dài.

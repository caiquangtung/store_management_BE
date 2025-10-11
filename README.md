# Store Management Backend API

## 📋 Tổng Quan

Backend API cho hệ thống quản lý cửa hàng được xây dựng bằng .NET 9 với kiến trúc **Clean Architecture** 4-layer, đảm bảo tính maintainability, testability và scalability cao.

### 🎯 Tính Năng Chính

- ✅ **Authentication & Authorization** - JWT Bearer Token với Role-based Access Control
- ✅ **User Management** - Quản lý người dùng hệ thống (Admin/Staff)
- ✅ **Customer Management** - Quản lý khách hàng với tìm kiếm và phân trang
- ✅ **Product Management** - Quản lý sản phẩm với danh mục
- ✅ **Category Management** - Quản lý danh mục sản phẩm
- ✅ **Supplier Management** - Quản lý nhà cung cấp
- 🚧 **Order Processing** - Xử lý đơn hàng (Đang phát triển)
- 🚧 **Payment Handling** - Quản lý thanh toán (Đang phát triển)
- 🚧 **Inventory Tracking** - Theo dõi tồn kho (Đang phát triển)
- 🚧 **Promotion System** - Hệ thống khuyến mãi (Đang phát triển)

### 🏗️ Kiến Trúc

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

## 🛠️ Technology Stack

- **Framework**: .NET 9
- **Database**: MySQL 8.0
- **ORM**: Entity Framework Core 8.0
- **Authentication**: JWT Bearer Token
- **Validation**: FluentValidation
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Built-in .NET Logging

## 🚀 Quick Start (Khởi động nhanh)

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL 8.0](https://dev.mysql.com/downloads/mysql/)
- Git (để clone repository)

### 1. Start MySQL Service

```bash
# macOS (with Homebrew)
brew services start mysql

# Linux
sudo systemctl start mysql

# Windows
net start MySQL80
```

### 2. Setup Database

```bash
# Create database (if not exists)
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS store_management;"

# Import database schema
mysql -u root -p store_management < "Store Management Full.sql"

# Verify database created
mysql -u root -p -e "USE store_management; SHOW TABLES;"
```

### 3. Configure Application

Update `StoreManagement.API/appsettings.Development.json` with your MySQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=YOUR_MYSQL_PASSWORD;"
  },
  "JwtSettings": {
    "Secret": "development_secret_key_not_for_production_use_only",
    "Issuer": "StoreManagementAPI",
    "Audience": "StoreManagementClient",
    "ExpireMinutes": 1440
  }
}
```

### 4. Run Application

```bash
cd store_management_BE
dotnet restore
dotnet run --project StoreManagement.API
```

### 5. Test API

```bash
# Test login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "123456"}'

# Or open Swagger UI
open http://localhost:5000/swagger
```

**Default Test Credentials:**

- **Admin:** username: `admin`, password: `123456`
- **Staff:** username: `staff01`, password: `123456`

## 📊 Project Status

### ✅ Completed Features

- **Authentication System**: JWT-based authentication with refresh tokens
- **User Management**: Full CRUD operations for system users
- **Customer Management**: Complete customer management with search functionality
- **Product Management**: Product CRUD with category relationships
- **Category Management**: Product category management
- **Supplier Management**: Supplier information management
- **Authorization**: Role-based access control (Admin/Staff)
- **API Documentation**: Swagger/OpenAPI integration
- **Validation**: FluentValidation for all endpoints
- **Error Handling**: Global exception middleware

### 🚧 In Development

- **Order Management**: Order processing system
- **Payment Processing**: Payment handling and tracking
- **Inventory Management**: Stock tracking and management
- **Promotion System**: Discount and promotion management

### 🎯 Architecture Benefits

- **Clean Architecture**: 4-layer separation for maintainability
- **Dependency Injection**: Proper DI configuration
- **Repository Pattern**: Clean data access layer
- **AutoMapper**: Consistent entity-DTO mapping
- **Async/Await**: Non-blocking database operations
- **Security**: JWT authentication + role-based authorization

---

## 📦 Cài Đặt Chi Tiết

### 1. Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL 8.0](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio Code](https://code.visualstudio.com/) hoặc [Visual Studio](https://visualstudio.microsoft.com/)

### 2. Clone Repository

```bash
git clone <repository-url>
cd store_management_BE
```

### 3. Database Setup

#### Tạo Database:

```sql
CREATE DATABASE store_management;
```

#### Import Schema:

```bash
mysql -u root -p store_management < "Store Management Full.sql"
```

### 4. Configuration

#### Cách 1: Sử dụng appsettings.Development.json (Khuyến nghị)

Edit `StoreManagement.API/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=YOUR_PASSWORD;"
  },
  "JwtSettings": {
    "Secret": "development_secret_key_not_for_production_use_only",
    "Issuer": "StoreManagementAPI",
    "Audience": "StoreManagementClient",
    "ExpireMinutes": 1440
  }
}
```

#### Cách 2: Tạo Local Configuration (Tùy chọn)

```bash
# Copy template
cp StoreManagement.API/appsettings.json StoreManagement.API/appsettings.Local.json

# Edit với thông tin thực tế
nano StoreManagement.API/appsettings.Local.json
```

**appsettings.Local.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=YOUR_PASSWORD;"
  },
  "JwtSettings": {
    "Secret": "608ee45f61d37d40755933aac33c11fc62a5d2d645f969e87b050b4feb2561ba",
    "Issuer": "StoreManagementAPI",
    "Audience": "StoreManagementClient",
    "ExpireMinutes": 60
  }
}
```

### 5. Restore Packages & Build

```bash
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run API
dotnet run --project StoreManagement.API
```

### 6. Verify Installation

API sẽ chạy trên:

- **HTTP**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`

## 🔧 Configuration Files

### appsettings.json (Base - Safe to commit)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=CHANGE_THIS_PASSWORD;"
  },
  "JwtSettings": {
    "Secret": "CHANGE_THIS_SECRET_KEY_IN_PRODUCTION_AT_LEAST_32_CHARACTERS",
    "Issuer": "StoreManagementAPI",
    "Audience": "StoreManagementClient",
    "ExpireMinutes": 60
  }
}
```

### appsettings.Development.json (Dev overrides)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=store_management_dev;Uid=root;Pwd=DEV_PASSWORD_HERE;"
  },
  "JwtSettings": {
    "Secret": "development_secret_key_not_for_production_use_only",
    "ExpireMinutes": 1440
  }
}
```

### appsettings.Local.json (NOT committed)

- Chứa thông tin thực tế của bạn
- Được ignore bởi Git
- Override tất cả configs khác

## 🚀 API Endpoints

### Authentication

- `POST /api/auth/login` - Đăng nhập (trả về token + refreshToken)
- `POST /api/auth/refresh` - Lấy token mới bằng refreshToken (rotate)
- `POST /api/auth/logout` - Thu hồi refreshToken hiện tại

### Users (Admin/Staff Only)

- `GET /api/users` - Lấy danh sách người dùng (paginated)
- `GET /api/users/{id}` - Lấy thông tin người dùng
- `POST /api/users` - Tạo người dùng mới
- `PUT /api/users/{id}` - Cập nhật người dùng
- `DELETE /api/users/{id}` - Xóa người dùng

### Customers (Staff/Admin Only)

- `GET /api/customers` - Lấy danh sách khách hàng (paginated + search)
- `GET /api/customers/{id}` - Lấy thông tin khách hàng
- `POST /api/customers` - Tạo khách hàng mới
- `PUT /api/customers/{id}` - Cập nhật khách hàng
- `DELETE /api/customers/{id}` - Xóa khách hàng

### Products (Staff/Admin Only)

- `GET /api/products` - Lấy danh sách sản phẩm (paginated)
- `GET /api/products/{id}` - Lấy thông tin sản phẩm
- `POST /api/products` - Tạo sản phẩm mới
- `PUT /api/products/{id}` - Cập nhật sản phẩm
- `DELETE /api/products/{id}` - Xóa sản phẩm

### Categories (Staff/Admin Only)

- `GET /api/categories` - Lấy danh sách danh mục (paginated)
- `GET /api/categories/{id}` - Lấy thông tin danh mục
- `POST /api/categories` - Tạo danh mục mới
- `PUT /api/categories/{id}` - Cập nhật danh mục
- `DELETE /api/categories/{id}` - Xóa danh mục

### Suppliers (Staff/Admin Only)

- `GET /api/suppliers` - Lấy danh sách nhà cung cấp (paginated)
- `GET /api/suppliers/{id}` - Lấy thông tin nhà cung cấp
- `POST /api/suppliers` - Tạo nhà cung cấp mới
- `PUT /api/suppliers/{id}` - Cập nhật nhà cung cấp
- `DELETE /api/suppliers/{id}` - Xóa nhà cung cấp

### 🚧 Coming Soon

- **Orders** - Quản lý đơn hàng
- **Payments** - Xử lý thanh toán
- **Inventory** - Theo dõi tồn kho
- **Promotions** - Hệ thống khuyến mãi

## 📊 Response Format

### Success Response

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data here
  },
  "error": null,
  "timestamp": "2025-01-01T00:00:00Z"
}
```

### Paginated Response

```json
{
  "success": true,
  "message": "Data retrieved successfully",
  "data": {
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "timestamp": "2025-01-01T00:00:00Z"
}
```

### Error Response

```json
{
  "success": false,
  "message": "Operation failed",
  "data": null,
  "error": "Detailed error message",
  "timestamp": "2025-01-01T00:00:00Z"
}
```

## 🔐 Authentication

### Login Request

```json
POST /api/auth/login
{
  "username": "admin",
  "password": "password123"
}
```

### Login Response

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string_here",
    "expiresAt": "2025-01-01T01:00:00Z",
    "user": {
      "userId": 1,
      "username": "admin",
      "fullName": "Administrator",
      "role": "Admin"
    }
  },
  "timestamp": "2025-01-01T00:00:00Z"
}
```

### Using JWT Token

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     http://localhost:5000/api/users
```

## 🗄️ Database Schema

### Core Tables (Currently Implemented)

- **users** - Người dùng hệ thống (admin, staff) ✅
- **customers** - Khách hàng ✅
- **categories** - Danh mục sản phẩm ✅
- **suppliers** - Nhà cung cấp ✅
- **products** - Sản phẩm ✅

### Planned Tables (Coming Soon)

- **inventory** - Tồn kho 🚧
- **promotions** - Khuyến mãi 🚧
- **orders** - Đơn hàng 🚧
- **order_items** - Chi tiết đơn hàng 🚧
- **payments** - Thanh toán 🚧

## 🧪 Testing

### Unit Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### API Testing với Swagger

1. Mở `http://localhost:5000/swagger`
2. Click "Authorize" và nhập JWT token
3. Test các endpoints

## 📝 Development Guidelines

### Code Structure

```
StoreManagement/
├── StoreManagement.Domain/           # Domain entities & interfaces (Core Layer)
│   ├── Entities/                     # Domain entities (User, Product, Category, etc.)
│   ├── Enums/                        # Enumerations (UserRole, OrderStatus, etc.)
│   └── Interfaces/                   # Repository interfaces (IRepository, IUserRepository, etc.)
├── StoreManagement.Application/      # Business logic & DTOs (Application Layer)
│   ├── Common/Interfaces/            # Application interfaces (IJwtService, IPasswordService)
│   ├── DTOs/                         # Data Transfer Objects
│   │   ├── Auth/                     # Authentication DTOs (LoginRequest, LoginResponse)
│   │   ├── Users/                    # User-related DTOs
│   │   ├── Customer/                 # Customer-related DTOs
│   │   ├── Product/                  # Product-related DTOs
│   │   ├── Category/                 # Category-related DTOs
│   │   └── Suppliers/                # Supplier-related DTOs
│   ├── Mappings/                     # AutoMapper profiles
│   ├── Services/                     # Application services (IAuthService, IUserService, etc.)
│   └── Validators/                   # FluentValidation validators
├── StoreManagement.Infrastructure/   # Data access & external services (Infrastructure Layer)
│   ├── Data/                         # DbContext & database configuration
│   ├── Extensions/                   # Service collection extensions (DI configuration)
│   ├── Models/                       # Infrastructure models (JwtSettings)
│   ├── Repositories/                 # Repository implementations
│   └── Services/                     # Infrastructure services (JwtService, PasswordService)
└── StoreManagement.API/              # Controllers & middleware (Presentation Layer)
    ├── Attributes/                   # Custom attributes (AuthorizeRoleAttribute)
    ├── Authorization/                # Authorization handlers & requirements
    ├── Controllers/                  # API controllers (AuthController, UsersController, etc.)
    ├── Middleware/                   # Custom middleware (GlobalExceptionMiddleware)
    └── Models/                       # API response models (ApiResponse, PagedResult)
```

### Best Practices

- ✅ **Clean Architecture**: Tuân thủ 4-layer architecture với dependency inversion
- ✅ **Consistent Response**: Sử dụng `ApiResponse<T>` cho consistent response format
- ✅ **Pagination**: Implement pagination cho tất cả list endpoints
- ✅ **Validation**: Sử dụng FluentValidation cho input validation
- ✅ **Error Handling**: Handle errors với GlobalExceptionMiddleware
- ✅ **Security**: JWT authentication + Role-based authorization
- ✅ **Async/Await**: Async/await pattern cho tất cả database operations
- ✅ **Dependency Injection**: Proper DI configuration và service registration
- ✅ **Repository Pattern**: Clean separation giữa business logic và data access
- ✅ **AutoMapper**: Consistent mapping giữa entities và DTOs

### Git Workflow

```bash
# Create feature branch
git checkout -b feature/user-management

# Make changes and commit
git add .
git commit -m "feat: add user management endpoints"

# Push and create PR
git push origin feature/user-management
```

## 🚀 Deployment

### Development

```bash
dotnet run --project StoreManagement.API --environment Development
```

### Production

```bash
# Build for production
dotnet publish -c Release -o ./publish

# Run with production config
dotnet ./publish/StoreManagement.API.dll --environment Production
```

### Environment Variables (Production)

```bash
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=store_management;Uid=prod_user;Pwd=SECURE_PASSWORD;"
export JwtSettings__Secret="PRODUCTION_SECRET_KEY_FROM_SECURE_STORE"
export ASPNETCORE_ENVIRONMENT=Production
```

## 🐛 Troubleshooting

### Common Issues

#### 1. Database Connection Failed

```bash
# Check MySQL service
sudo systemctl status mysql

# Test connection
mysql -u root -p -e "SELECT 1;"
```

#### 2. Port Already in Use

```bash
# Find process using port 5000
lsof -i :5000

# Kill process
kill -9 <PID>

# Or use different port
dotnet run --project StoreManagement.API --urls "http://localhost:5001"
```

#### 3. Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## 📚 Documentation

### Project Documentation

- 🏗️ [Project Layer Architecture & References](doc/Project_Layer_Architecture_and_References.md) - Chi tiết về kiến trúc 4-layer và dependencies
- 📖 [Authentication Implementation](doc/Authentication_Implementation.md) - Chi tiết về hệ thống xác thực JWT
- 🔐 [Authorization Implementation](doc/Authorization_Implementation.md) - Hệ thống phân quyền Role-based Access Control
- 👥 [User Management Implementation](doc/User_Management_Implementation.md) - Quản lý người dùng hệ thống
- 🛒 [Customer API Implementation](doc/Customer_API_Implementation.md) - API quản lý khách hàng
- 📋 [Development Plan](doc/Store_Management_Backend_Development_Plan.md) - Kế hoạch phát triển dự án

### External Resources

- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [JWT Authentication](https://jwt.io/)
- [MySQL Documentation](https://dev.mysql.com/doc/)

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Team

- **Backend Developer**: [Your Name]
- **Database Designer**: [Your Name]
- **DevOps**: [Your Name]

## 📞 Support

Nếu có vấn đề hoặc câu hỏi, vui lòng tạo issue trên GitHub hoặc liên hệ team development.

---

**Happy Coding! 🚀**

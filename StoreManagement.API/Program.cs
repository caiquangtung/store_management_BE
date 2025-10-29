using Microsoft.EntityFrameworkCore;
using StoreManagement.Infrastructure.Data;
using StoreManagement.API.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using StoreManagement.Application.Validators;
using StoreManagement.Infrastructure.Models;
using StoreManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using StoreManagement.Application.Services;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Repositories;
using StoreManagement.Infrastructure.Extensions;
using StoreManagement.Application.Common.Interfaces;
using StoreManagement.Application.Mappings;
using AutoMapper;
using StoreManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using StoreManagement.Domain.Entities;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    // Get allowed origins from configuration
    var corsSettings = builder.Configuration.GetSection("CorsSettings");
    var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new string[]
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:5173",
        "http://localhost:4200",
        "http://localhost:8080",
        "http://127.0.0.1:3000",
        "http://127.0.0.1:5173",
        "https://localhost:3000",
        "https://localhost:5173"
    };

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Allow cookies and authorization headers
    });
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCategoryRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSupplierRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateSupplierRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCustomerRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePromotionRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdatePromotionRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ValidatePromotionRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateInventoryRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddOrderItemRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateOrderItemRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ApplyPromotionRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CheckoutRequestValidator>();

// Add DbContext with connection string from appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0))
    ));

// Configure JWT settings from appsettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Add JWT Authentication
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

// Add Authorization with role-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new StoreManagement.API.Authorization.UserRoleRequirement(UserRole.Admin)));

    options.AddPolicy("AdminOrStaff", policy =>
        policy.Requirements.Add(new StoreManagement.API.Authorization.UserRoleRequirement(UserRole.Admin, UserRole.Staff)));

    options.AddPolicy("AllRoles", policy =>
        policy.Requirements.Add(new StoreManagement.API.Authorization.UserRoleRequirement(UserRole.Admin, UserRole.Staff)));
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, StoreManagement.API.Authorization.UserRoleHandler>();

// Register JWT Service with JwtSettings instance
builder.Services.AddScoped<IJwtService, JwtService>(provider =>
{
    var jwtSettings = provider.GetRequiredService<IOptions<JwtSettings>>().Value;
    return new JwtService(jwtSettings);
});

// Register Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(
    typeof(StoreManagement.Application.Mappings.UserMappingProfile),
    typeof(StoreManagement.Application.Mappings.ProductMappingProfile),
    typeof(StoreManagement.Application.Mappings.CategoryMappingProfile),
    typeof(StoreManagement.Application.Mappings.SupplierMappingProfile),
    typeof(StoreManagement.Application.Mappings.CustomerMappingProfile),
    typeof(StoreManagement.Application.Mappings.PromotionMappingProfile),
    typeof(StoreManagement.Application.Mappings.InventoryMappingProfile),
    typeof(StoreManagement.Application.Mappings.OrderMappingProfile));

// Register Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<IRepository<Supplier>, SupplierRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add Global Exception Middleware (should be early in pipeline)
app.UseGlobalExceptionMiddleware();

// Add CORS (must be before UseAuthentication and UseAuthorization)
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

// Add Static Files to serve images from wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")
    ),
    RequestPath = "/images"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
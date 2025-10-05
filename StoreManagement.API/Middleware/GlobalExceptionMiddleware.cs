using System.Net;
using System.Text.Json;
using StoreManagement.API.Models;

namespace StoreManagement.API.Middleware;

/// <summary>
/// Global exception handling middleware for centralized error handling
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse();

        switch (exception)
        {
            case ArgumentException argEx:
                response = ApiResponse.ValidationErrorResponse(argEx.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                response = ApiResponse.ErrorResponse("Unauthorized access", "Access denied");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case KeyNotFoundException:
                response = ApiResponse.ErrorResponse("Resource not found", "The requested resource was not found");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case InvalidOperationException invalidOpEx:
                response = ApiResponse.ErrorResponse(invalidOpEx.Message, "Invalid operation");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case MySqlConnector.MySqlException mysqlEx:
                response = HandleMySqlException(mysqlEx);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;

            default:
                response = ApiResponse.ErrorResponse(
                    "An unexpected error occurred",
                    "Internal server error"
                );
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static ApiResponse HandleMySqlException(MySqlConnector.MySqlException ex)
    {
        return ex.Number switch
        {
            1042 => ApiResponse.ErrorResponse("Database connection failed", "Unable to connect to database"),
            1045 => ApiResponse.ErrorResponse("Database authentication failed", "Invalid database credentials"),
            1146 => ApiResponse.ErrorResponse("Table not found", "Database table does not exist"),
            1062 => ApiResponse.ErrorResponse("Duplicate entry", "Record already exists"),
            1452 => ApiResponse.ErrorResponse("Foreign key constraint failed", "Invalid reference to related record"),
            _ => ApiResponse.ErrorResponse($"Database error: {ex.Message}", "Database operation failed")
        };
    }
}

/// <summary>
/// Extension method to register GlobalExceptionMiddleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

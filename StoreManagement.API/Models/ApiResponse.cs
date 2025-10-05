namespace StoreManagement.API.Models;

/// <summary>
/// Standard API response wrapper for consistent response format
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string error, string message = "Operation failed")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = error
        };
    }

    public static ApiResponse<T> ValidationErrorResponse(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Error = error
        };
    }

    public static ApiResponse<T> ValidationErrorResponse(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
    }
}

/// <summary>
/// Non-generic API response for operations that don't return data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResponse(string message = "Operation completed successfully")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public new static ApiResponse ErrorResponse(string error, string message = "Operation failed")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Error = error
        };
    }

    public new static ApiResponse ValidationErrorResponse(string error)
    {
        return new ApiResponse
        {
            Success = false,
            Message = "Validation failed",
            Error = error
        };
    }

    public new static ApiResponse ValidationErrorResponse(List<string> errors)
    {
        return new ApiResponse
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
    }
}

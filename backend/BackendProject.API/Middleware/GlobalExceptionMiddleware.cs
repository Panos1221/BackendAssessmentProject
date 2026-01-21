using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace BackendProject.API.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, 
            "Unhandled exception: {ExceptionType} - {Message} | Path: {Path} | Method: {Method}",
            exception.GetType().Name, 
            exception.Message,
            context.Request.Path,
            context.Request.Method);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Validation failed";
                errorResponse.Errors = validationException.Errors
                    .Select(e => new ValidationError
                    {
                        Property = e.PropertyName,
                        Message = e.ErrorMessage
                    })
                    .ToList();
                break;

            case DbUpdateException dbUpdateException:
                var sqlException = dbUpdateException.InnerException as SqlException;
                if (sqlException != null)
                {
                    switch (sqlException.Number)
                    {
                        case 2627: // Unique constraint violation
                            response.StatusCode = (int)HttpStatusCode.Conflict;
                            errorResponse.Message = "A record with this value already exists. Please use a unique value.";
                            _logger.LogWarning("Unique constraint violation: {Message}", sqlException.Message);
                            break;
                        case 547: // Foreign key constraint violation
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            errorResponse.Message = "The operation cannot be completed because it would violate referential integrity.";
                            _logger.LogWarning("Foreign key constraint violation: {Message}", sqlException.Message);
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            errorResponse.Message = "A database error occurred while processing your request.";
                            _logger.LogError(dbUpdateException, "Database error: {SqlErrorNumber} - {Message}", sqlException.Number, sqlException.Message);
                            break;
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "A database error occurred while processing your request.";
                    _logger.LogError(dbUpdateException, "Database update exception without SQL exception");
                }
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = exception.Message;
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = exception.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An internal server error occurred";
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? Errors { get; set; }
}

public class ValidationError
{
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

using Microsoft.EntityFrameworkCore;
using PetShop.Api.DTOs;
using PetShop.Domain;
using System.Net;
using System.Text.Json;

namespace PetShop.Api.Middleware;

/// <summary>
/// Global exception handler middleware that catches all exceptions and returns consistent error responses.
/// Maps domain exceptions to appropriate HTTP status codes and formats error responses according to the API contract.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        var (statusCode, errorCode, message, details) = MapException(exception);

        var errorResponse = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = errorCode,
                Message = message,
                Details = details
            }
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode StatusCode, string ErrorCode, string Message, Dictionary<string, object>? Details) MapException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                GetNotFoundErrorCode(exception),
                exception.Message,
                null
            ),
            InvalidOrderStateException => (
                HttpStatusCode.Conflict,
                "ORDER_INVALID_STATE",
                exception.Message,
                null
            ),
            BusinessRuleViolationException businessEx => (
                DetermineBusinessRuleStatusCode(businessEx),
                "BUSINESS_RULE_VIOLATION",
                businessEx.Message,
                null
            ),
            ArgumentNullException argEx => (
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                $"Required parameter '{argEx.ParamName}' cannot be null.",
                new Dictionary<string, object> { { "parameter", argEx.ParamName ?? "unknown" } }
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                argEx.Message,
                argEx.ParamName != null
                    ? new Dictionary<string, object> { { "parameter", argEx.ParamName } }
                    : null
            ),
            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                "CONCURRENCY_ERROR",
                "The resource was modified by another operation. Please refresh and try again.",
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred. Please try again later.",
                null
            )
        };
    }

    private static string GetNotFoundErrorCode(Exception exception)
    {
        var message = exception.Message.ToLowerInvariant();

        if (message.Contains("customer"))
            return "CUSTOMER_NOT_FOUND";

        if (message.Contains("order"))
            return "ORDER_NOT_FOUND";

        if (message.Contains("pet"))
            return "PET_NOT_FOUND";

        return "RESOURCE_NOT_FOUND";
    }

    private static HttpStatusCode DetermineBusinessRuleStatusCode(BusinessRuleViolationException exception)
    {
        // Most business rule violations are conflicts (409)
        // Semantic validation errors (like invalid dates) should be 422
        // For now, default to 409. Can be enhanced later based on exception message or custom properties.
        var message = exception.Message.ToLowerInvariant();

        if (message.Contains("date") && (message.Contains("past") || message.Contains("invalid")))
            return HttpStatusCode.UnprocessableEntity;

        return HttpStatusCode.Conflict;
    }
}

using System.Text.Json;
using FluentValidation;
using KidsCartoonPipeline.Core.Exceptions;

namespace KidsCartoonPipeline.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        var (statusCode, type, title) = exception switch
        {
            NotFoundException => (404, "NotFoundException", "Resource not found"),
            ExternalServiceException => (502, "ExternalServiceException", "External service error"),
            ValidationException => (422, "ValidationException", "Validation failed"),
            InvalidOperationException => (400, "InvalidOperationException", "Invalid operation"),
            _ => (500, "InternalServerError", "An unexpected error occurred")
        };

        if (statusCode == 500)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Type}", type);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type,
            title,
            status = statusCode,
            detail = exception.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

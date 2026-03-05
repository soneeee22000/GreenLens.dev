using System.Text.Json;
using GreenLens.Core.Models;
using GreenLens.Shared.Constants;
using GreenLens.Shared.DTOs;

namespace GreenLens.Api.Middleware;

/// <summary>
/// Global exception handler that converts exceptions to consistent API responses.
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Application error: {Code} - {Message}", ex.Code, ex.Message);
            await WriteErrorResponse(context, ex.StatusCode, ex.Code, ex.Message, ex.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(
                context,
                500,
                ErrorCodes.InternalError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string code,
        string message,
        string[]? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        if (statusCode == 503)
        {
            context.Response.Headers.Append("Retry-After", "30");
        }

        var response = new ApiResponse<object>(
            Data: null,
            Error: new ApiError(code, message, details));

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

using GreenLens.Shared.Constants;
using GreenLens.Shared.DTOs;
using System.Text.Json;

namespace GreenLens.Api.Middleware;

/// <summary>
/// Middleware that validates API key from X-Api-Key header.
/// Skips auth for health check and Swagger endpoints.
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;
    private static readonly HashSet<string> SkipPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
        "/api/v1/regions"
    };

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["API_KEY"]
            ?? throw new InvalidOperationException("API_KEY is not configured.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (ShouldSkipAuth(path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedKey)
            || providedKey.ToString() != _apiKey)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>(
                Data: null,
                Error: new ApiError(ErrorCodes.Unauthorized, "Invalid or missing API key."));

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
            return;
        }

        await _next(context);
    }

    private static bool ShouldSkipAuth(string path)
    {
        return SkipPaths.Any(skip => path.StartsWith(skip, StringComparison.OrdinalIgnoreCase));
    }
}

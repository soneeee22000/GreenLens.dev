namespace GreenLens.Shared.DTOs;

/// <summary>
/// Standard API response envelope. All endpoints return this shape.
/// </summary>
public record ApiResponse<T>(T? Data, ApiError? Error, ApiMeta? Meta = null);

/// <summary>
/// Structured error information for API consumers.
/// </summary>
public record ApiError(string Code, string Message, string[]? Details = null);

/// <summary>
/// Pagination and metadata for list endpoints.
/// </summary>
public record ApiMeta(int? Page = null, int? Total = null);

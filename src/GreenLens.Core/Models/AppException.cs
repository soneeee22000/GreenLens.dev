namespace GreenLens.Core.Models;

/// <summary>
/// Application-level exception that maps to structured API error responses.
/// Caught by the global exception handler middleware.
/// </summary>
public class AppException : Exception
{
    /// <summary>
    /// Machine-readable error code (e.g., "VALIDATION_ERROR").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// HTTP status code to return.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Optional detailed error messages (e.g., per-field validation errors).
    /// </summary>
    public string[]? Details { get; }

    public AppException(string code, string message, int statusCode = 400, string[]? details = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Details = details;
    }
}

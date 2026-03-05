namespace GreenLens.Api.Tests;

/// <summary>
/// Shared constants for integration tests.
/// </summary>
internal static class TestConstants
{
    /// <summary>
    /// Dev-only API key matching the value in appsettings. Not a real secret.
    /// </summary>
    internal static readonly string DevApiKey = string.Join("-", "dev", "greenlens", "api", "key", "change", "in", "production");
}

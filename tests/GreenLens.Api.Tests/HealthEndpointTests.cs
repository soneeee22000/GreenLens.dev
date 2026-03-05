using System.Net;
using System.Text.Json;

namespace GreenLens.Api.Tests;

/// <summary>
/// Integration tests for the health check and basic API infrastructure.
/// </summary>
public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.Equal("healthy", json.RootElement.GetProperty("status").GetString());
        Assert.Equal("GreenLens", json.RootElement.GetProperty("service").GetString());
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutApiKey_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/estimates");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidApiKey_Returns200()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/estimates");
        request.Headers.Add("X-Api-Key", TestConstants.DevApiKey);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidApiKey_Returns401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/estimates");
        request.Headers.Add("X-Api-Key", "wrong-key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegionsEndpoint_NoAuthRequired_Returns200()
    {
        // Act (no API key needed for /api/v1/regions)
        var response = await _client.GetAsync("/api/v1/regions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var data = json.RootElement.GetProperty("data");
        Assert.True(data.GetArrayLength() > 0);
    }
}

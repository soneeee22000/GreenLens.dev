using System.Net;
using System.Text;
using System.Text.Json;

namespace GreenLens.Api.Tests;

/// <summary>
/// Integration tests for the GET /api/v1/estimates/{id}/recommendations endpoint.
/// </summary>
public class RecommendationsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RecommendationsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRecommendations_WithValidEstimate_Returns200WithRecommendations()
    {
        // Arrange: create an estimate first
        var estimateId = await CreateTestEstimate();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/estimates/{estimateId}/recommendations");
        request.Headers.Add("X-Api-Key", TestConstants.DevApiKey);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var data = json.RootElement.GetProperty("data");
        Assert.True(data.GetArrayLength() >= 3);

        // Verify each recommendation has required fields
        foreach (var rec in data.EnumerateArray())
        {
            Assert.False(string.IsNullOrWhiteSpace(rec.GetProperty("title").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(rec.GetProperty("description").GetString()));
            Assert.True(rec.GetProperty("estimatedReductionPercent").GetDecimal() >= 0);
            Assert.Contains(rec.GetProperty("effort").GetString(), new[] { "Low", "Medium", "High" });
        }
    }

    [Fact]
    public async Task GetRecommendations_WithNonExistentEstimate_Returns404()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/estimates/{fakeId}/recommendations");
        request.Headers.Add("X-Api-Key", TestConstants.DevApiKey);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRecommendations_WithoutApiKey_Returns401()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/estimates/{fakeId}/recommendations");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> CreateTestEstimate()
    {
        var body = JsonSerializer.Serialize(new
        {
            resources = new[]
            {
                new
                {
                    resourceType = "Standard_D4s_v3",
                    quantity = 2,
                    hours = 720,
                    region = "westeurope"
                }
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/estimates")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Api-Key", TestConstants.DevApiKey);

        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        return json.RootElement.GetProperty("data").GetProperty("estimateId").GetGuid();
    }
}

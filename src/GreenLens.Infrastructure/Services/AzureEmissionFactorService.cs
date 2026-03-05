using System.Text.Json.Serialization;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using GreenLens.Core.Interfaces;
using GreenLens.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenLens.Infrastructure.Services;

/// <summary>
/// Azure AI Search implementation of the emission factor service.
/// Queries the emission-factors index for resource type lookups and natural language search.
/// </summary>
public class AzureEmissionFactorService : IEmissionFactorService
{
    private readonly SearchClient _searchClient;
    private readonly ILogger<AzureEmissionFactorService> _logger;

    public AzureEmissionFactorService(
        IConfiguration configuration,
        ILogger<AzureEmissionFactorService> logger)
    {
        _logger = logger;

        var endpoint = configuration["AZURE_SEARCH_ENDPOINT"]
            ?? throw new InvalidOperationException("AZURE_SEARCH_ENDPOINT is not configured.");
        var apiKey = configuration["AZURE_SEARCH_API_KEY"]
            ?? throw new InvalidOperationException("AZURE_SEARCH_API_KEY is not configured.");
        var indexName = configuration["AZURE_SEARCH_INDEX_NAME"] ?? "emission-factors";

        _searchClient = new SearchClient(
            new Uri(endpoint),
            indexName,
            new AzureKeyCredential(apiKey));
    }

    /// <inheritdoc />
    public async Task<EmissionFactor?> GetFactorAsync(
        string resourceType,
        string region,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new SearchOptions
            {
                Filter = $"resourceType eq '{resourceType}'",
                Size = 1,
                Select = { "id", "resourceType", "provider", "region", "co2ePerUnit", "unit", "source", "effectiveDate" }
            };

            var results = await _searchClient.SearchAsync<SearchEmissionFactor>(
                resourceType, options, cancellationToken);

            await foreach (var result in results.Value.GetResultsAsync())
            {
                return MapToDomain(result.Document);
            }

            // Fallback: try without exact filter (partial match)
            options.Filter = null;
            options.Size = 1;
            results = await _searchClient.SearchAsync<SearchEmissionFactor>(
                resourceType, options, cancellationToken);

            await foreach (var result in results.Value.GetResultsAsync())
            {
                return MapToDomain(result.Document);
            }

            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure AI Search request failed for resource type {ResourceType}", resourceType);
            throw new Core.Models.AppException(
                "SERVICE_UNAVAILABLE",
                "Emission factor search is currently unavailable. Please try again later.",
                503);
        }
    }

    /// <inheritdoc />
    public async Task<List<EmissionFactor>> SearchAsync(
        string query,
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new SearchOptions
            {
                Size = top,
                Select = { "id", "resourceType", "provider", "region", "co2ePerUnit", "unit", "source", "effectiveDate" }
            };

            var results = await _searchClient.SearchAsync<SearchEmissionFactor>(
                query, options, cancellationToken);

            var factors = new List<EmissionFactor>();
            await foreach (var result in results.Value.GetResultsAsync())
            {
                factors.Add(MapToDomain(result.Document));
            }

            return factors;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure AI Search query failed: {Query}", query);
            throw new Core.Models.AppException(
                "SERVICE_UNAVAILABLE",
                "Emission factor search is currently unavailable. Please try again later.",
                503);
        }
    }

    private static EmissionFactor MapToDomain(SearchEmissionFactor doc)
    {
        return new EmissionFactor
        {
            Id = doc.Id ?? string.Empty,
            ResourceType = doc.ResourceType ?? string.Empty,
            Provider = doc.Provider ?? string.Empty,
            Region = doc.Region ?? string.Empty,
            Co2ePerUnit = (decimal)(doc.Co2ePerUnit ?? 0),
            Unit = doc.Unit ?? string.Empty,
            Source = doc.Source ?? string.Empty,
            EffectiveDate = doc.EffectiveDate?.DateTime ?? DateTime.MinValue
        };
    }
}

/// <summary>
/// Azure AI Search document model for emission factors.
/// Field names match the search index schema.
/// </summary>
public class SearchEmissionFactor
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("resourceType")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("co2ePerUnit")]
    public double? Co2ePerUnit { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("effectiveDate")]
    public DateTimeOffset? EffectiveDate { get; set; }
}

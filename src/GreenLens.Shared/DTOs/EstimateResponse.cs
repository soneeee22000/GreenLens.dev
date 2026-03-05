namespace GreenLens.Shared.DTOs;

/// <summary>
/// Response body for a completed carbon footprint estimate.
/// </summary>
public record EstimateResponse
{
    public required Guid EstimateId { get; init; }
    public required decimal TotalCo2eKg { get; init; }
    public required List<ResourceEstimateResponse> Breakdown { get; init; }
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// Per-resource CO2e breakdown within an estimate.
/// </summary>
public record ResourceEstimateResponse
{
    public required string ResourceType { get; init; }
    public required int Quantity { get; init; }
    public required decimal Hours { get; init; }
    public required string Region { get; init; }
    public required decimal Co2eKg { get; init; }
    public required decimal Co2ePerUnit { get; init; }
    public required string Unit { get; init; }
}

/// <summary>
/// AI-generated recommendation for reducing carbon footprint.
/// </summary>
public record RecommendationResponse
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required decimal EstimatedReductionPercent { get; init; }
    public required string Effort { get; init; }
}

/// <summary>
/// Emission factor search result from Azure AI Search.
/// </summary>
public record EmissionFactorResponse
{
    public required string ResourceType { get; init; }
    public required string Provider { get; init; }
    public required string Region { get; init; }
    public required decimal Co2ePerUnit { get; init; }
    public required string Unit { get; init; }
    public required string Source { get; init; }
    public required DateTime LastUpdated { get; init; }
}

/// <summary>
/// Supported region with grid carbon intensity.
/// </summary>
public record RegionResponse
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required decimal GridCarbonIntensityGCo2ePerKwh { get; init; }
}

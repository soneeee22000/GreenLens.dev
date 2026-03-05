namespace GreenLens.Core.Models;

/// <summary>
/// Domain entity representing a completed carbon footprint estimate.
/// </summary>
public class CarbonEstimate
{
    public Guid Id { get; set; }
    public decimal TotalCo2eKg { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ResourceUsage> Resources { get; set; } = new();
}

/// <summary>
/// Domain entity representing a single resource's contribution to an estimate.
/// </summary>
public class ResourceUsage
{
    public Guid Id { get; set; }
    public Guid CarbonEstimateId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Hours { get; set; }
    public string Region { get; set; } = string.Empty;
    public decimal Co2eKg { get; set; }
    public decimal Co2ePerUnit { get; set; }
    public string Unit { get; set; } = string.Empty;
}

namespace GreenLens.Core.Models;

/// <summary>
/// Domain entity representing an emission factor for a cloud resource type.
/// Maps to Azure AI Search index documents.
/// </summary>
public class EmissionFactor
{
    public string Id { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal Co2ePerUnit { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
}

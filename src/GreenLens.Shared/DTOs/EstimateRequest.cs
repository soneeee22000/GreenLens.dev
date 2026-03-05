using System.ComponentModel.DataAnnotations;

namespace GreenLens.Shared.DTOs;

/// <summary>
/// Request body for creating a carbon footprint estimate.
/// </summary>
public record EstimateRequest
{
    /// <summary>
    /// List of cloud resources to estimate. Maximum 50 items.
    /// </summary>
    [Required]
    [MaxLength(50, ErrorMessage = "Maximum 50 resources per request.")]
    public required List<ResourceUsageRequest> Resources { get; init; }
}

/// <summary>
/// Individual cloud resource usage for estimation.
/// </summary>
public record ResourceUsageRequest
{
    /// <summary>
    /// Azure resource type (e.g., "Standard_D4s_v3", "BlobStorage", "AppServicePlan_S1").
    /// </summary>
    [Required]
    public required string ResourceType { get; init; }

    /// <summary>
    /// Number of instances of this resource.
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10,000.")]
    public required int Quantity { get; init; }

    /// <summary>
    /// Hours of usage for compute resources. Use 0 for storage resources.
    /// </summary>
    [Range(0, 744, ErrorMessage = "Hours must be between 0 and 744 (max hours in a month).")]
    public required decimal Hours { get; init; }

    /// <summary>
    /// Azure region (e.g., "westeurope", "eastus", "southeastasia").
    /// </summary>
    [Required]
    public required string Region { get; init; }
}

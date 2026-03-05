using GreenLens.Core.Interfaces;
using GreenLens.Core.Models;
using GreenLens.Shared.Constants;
using GreenLens.Shared.DTOs;

namespace GreenLens.Core.Services;

/// <summary>
/// Core business logic for calculating carbon footprint from cloud resource usage.
/// </summary>
public class CarbonEstimationService
{
    private readonly IEmissionFactorService _emissionFactorService;
    private readonly IEstimateRepository _estimateRepository;

    public CarbonEstimationService(
        IEmissionFactorService emissionFactorService,
        IEstimateRepository estimateRepository)
    {
        _emissionFactorService = emissionFactorService;
        _estimateRepository = estimateRepository;
    }

    /// <summary>
    /// Calculates carbon estimate for the given resource usage and persists the result.
    /// </summary>
    public async Task<CarbonEstimate> EstimateAsync(
        EstimateRequest request,
        CancellationToken cancellationToken = default)
    {
        var estimate = new CarbonEstimate
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Resources = new List<ResourceUsage>()
        };

        foreach (var resource in request.Resources)
        {
            var factor = await _emissionFactorService.GetFactorAsync(
                resource.ResourceType,
                resource.Region,
                cancellationToken);

            if (factor is null)
            {
                throw new AppException(
                    ErrorCodes.UnsupportedResourceType,
                    $"Unsupported resource type: {resource.ResourceType} in region {resource.Region}. " +
                    "See /api/v1/emission-factors for supported types.",
                    400);
            }

            var co2eKg = CalculateCo2e(factor, resource.Quantity, resource.Hours);

            estimate.Resources.Add(new ResourceUsage
            {
                Id = Guid.NewGuid(),
                CarbonEstimateId = estimate.Id,
                ResourceType = resource.ResourceType,
                Quantity = resource.Quantity,
                Hours = resource.Hours,
                Region = resource.Region,
                Co2eKg = co2eKg,
                Co2ePerUnit = factor.Co2ePerUnit,
                Unit = factor.Unit
            });
        }

        estimate.TotalCo2eKg = estimate.Resources.Sum(r => r.Co2eKg);

        return await _estimateRepository.CreateAsync(estimate, cancellationToken);
    }

    /// <summary>
    /// Calculates CO2e in kg for a resource based on its emission factor.
    /// For compute resources (unit = "kgCO2e/hour"): CO2e = factor * quantity * hours.
    /// For storage resources (unit = "kgCO2e/GB/month"): CO2e = factor * quantity.
    /// </summary>
    internal static decimal CalculateCo2e(EmissionFactor factor, int quantity, decimal hours)
    {
        return factor.Unit switch
        {
            "kgCO2e/hour" => factor.Co2ePerUnit * quantity * hours,
            "kgCO2e/GB/month" => factor.Co2ePerUnit * quantity,
            "kgCO2e/unit/month" => factor.Co2ePerUnit * quantity,
            _ => factor.Co2ePerUnit * quantity * hours
        };
    }
}

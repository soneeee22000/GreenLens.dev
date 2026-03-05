using GreenLens.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GreenLens.Api.Controllers;

/// <summary>
/// Endpoints for listing supported Azure regions and their grid carbon intensity.
/// Public endpoint (no auth required).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class RegionsController : ControllerBase
{
    /// <summary>
    /// Grid carbon intensity by Azure region in gCO2e/kWh.
    /// Sources: Electricity Maps, IEA, Azure sustainability reports.
    /// </summary>
    private static readonly List<RegionResponse> SupportedRegions = new()
    {
        new() { Name = "westeurope", DisplayName = "West Europe (Netherlands)", GridCarbonIntensityGCo2ePerKwh = 328 },
        new() { Name = "northeurope", DisplayName = "North Europe (Ireland)", GridCarbonIntensityGCo2ePerKwh = 267 },
        new() { Name = "eastus", DisplayName = "East US (Virginia)", GridCarbonIntensityGCo2ePerKwh = 336 },
        new() { Name = "eastus2", DisplayName = "East US 2 (Virginia)", GridCarbonIntensityGCo2ePerKwh = 336 },
        new() { Name = "westus", DisplayName = "West US (California)", GridCarbonIntensityGCo2ePerKwh = 196 },
        new() { Name = "westus2", DisplayName = "West US 2 (Washington)", GridCarbonIntensityGCo2ePerKwh = 78 },
        new() { Name = "centralus", DisplayName = "Central US (Iowa)", GridCarbonIntensityGCo2ePerKwh = 397 },
        new() { Name = "southeastasia", DisplayName = "Southeast Asia (Singapore)", GridCarbonIntensityGCo2ePerKwh = 408 },
        new() { Name = "japaneast", DisplayName = "Japan East (Tokyo)", GridCarbonIntensityGCo2ePerKwh = 462 },
        new() { Name = "uksouth", DisplayName = "UK South (London)", GridCarbonIntensityGCo2ePerKwh = 207 },
        new() { Name = "australiaeast", DisplayName = "Australia East (Sydney)", GridCarbonIntensityGCo2ePerKwh = 550 },
        new() { Name = "canadacentral", DisplayName = "Canada Central (Toronto)", GridCarbonIntensityGCo2ePerKwh = 26 },
        new() { Name = "francecentral", DisplayName = "France Central (Paris)", GridCarbonIntensityGCo2ePerKwh = 56 },
        new() { Name = "germanywestcentral", DisplayName = "Germany West Central (Frankfurt)", GridCarbonIntensityGCo2ePerKwh = 338 },
        new() { Name = "swedencentral", DisplayName = "Sweden Central (Gavle)", GridCarbonIntensityGCo2ePerKwh = 8 },
    };

    /// <summary>
    /// List all supported Azure regions with their grid carbon intensity.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<RegionResponse>>), 200)]
    public IActionResult List()
    {
        return Ok(new ApiResponse<List<RegionResponse>>(SupportedRegions, null));
    }
}

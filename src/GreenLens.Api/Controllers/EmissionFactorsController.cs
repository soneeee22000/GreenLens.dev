using GreenLens.Core.Interfaces;
using GreenLens.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GreenLens.Api.Controllers;

/// <summary>
/// Endpoints for searching and retrieving emission factor data.
/// </summary>
[ApiController]
[Route("api/v1/emission-factors")]
[Produces("application/json")]
public class EmissionFactorsController : ControllerBase
{
    private readonly IEmissionFactorService _emissionFactorService;

    public EmissionFactorsController(IEmissionFactorService emissionFactorService)
    {
        _emissionFactorService = emissionFactorService;
    }

    /// <summary>
    /// Search emission factors using natural language query via Azure AI Search.
    /// </summary>
    /// <param name="q">Natural language search query (e.g., "carbon cost of D4s VM")</param>
    /// <param name="top">Maximum results to return (default 10, max 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<EmissionFactorResponse>>), 200)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ApiResponse<object>(
                null,
                new ApiError("VALIDATION_ERROR", "Query parameter 'q' is required.")));
        }

        top = Math.Clamp(top, 1, 50);

        var factors = await _emissionFactorService.SearchAsync(q, top, cancellationToken);

        var responses = factors.Select(f => new EmissionFactorResponse
        {
            ResourceType = f.ResourceType,
            Provider = f.Provider,
            Region = f.Region,
            Co2ePerUnit = f.Co2ePerUnit,
            Unit = f.Unit,
            Source = f.Source,
            LastUpdated = f.EffectiveDate
        }).ToList();

        return Ok(new ApiResponse<List<EmissionFactorResponse>>(responses, null));
    }

    /// <summary>
    /// Get emission factor for a specific resource type.
    /// </summary>
    [HttpGet("{resourceType}")]
    [ProducesResponseType(typeof(ApiResponse<EmissionFactorResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetByResourceType(
        string resourceType,
        [FromQuery] string region = "global",
        CancellationToken cancellationToken = default)
    {
        var factor = await _emissionFactorService.GetFactorAsync(resourceType, region, cancellationToken);

        if (factor is null)
        {
            return NotFound(new ApiResponse<object>(
                null,
                new ApiError("NOT_FOUND", $"No emission factor found for resource type '{resourceType}'.")));
        }

        var response = new EmissionFactorResponse
        {
            ResourceType = factor.ResourceType,
            Provider = factor.Provider,
            Region = factor.Region,
            Co2ePerUnit = factor.Co2ePerUnit,
            Unit = factor.Unit,
            Source = factor.Source,
            LastUpdated = factor.EffectiveDate
        };

        return Ok(new ApiResponse<EmissionFactorResponse>(response, null));
    }
}

using GreenLens.Core.Interfaces;
using GreenLens.Core.Services;
using GreenLens.Shared.Constants;
using GreenLens.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GreenLens.Api.Controllers;

/// <summary>
/// Endpoints for creating and retrieving carbon footprint estimates.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class EstimatesController : ControllerBase
{
    private readonly CarbonEstimationService _estimationService;
    private readonly IEstimateRepository _estimateRepository;

    public EstimatesController(
        CarbonEstimationService estimationService,
        IEstimateRepository estimateRepository)
    {
        _estimationService = estimationService;
        _estimateRepository = estimateRepository;
    }

    /// <summary>
    /// Submit cloud resource usage and receive a carbon footprint estimate.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EstimateResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] EstimateRequest request,
        CancellationToken cancellationToken)
    {
        var estimate = await _estimationService.EstimateAsync(request, cancellationToken);

        var response = new EstimateResponse
        {
            EstimateId = estimate.Id,
            TotalCo2eKg = estimate.TotalCo2eKg,
            CreatedAt = estimate.CreatedAt,
            Breakdown = estimate.Resources.Select(r => new ResourceEstimateResponse
            {
                ResourceType = r.ResourceType,
                Quantity = r.Quantity,
                Hours = r.Hours,
                Region = r.Region,
                Co2eKg = r.Co2eKg,
                Co2ePerUnit = r.Co2ePerUnit,
                Unit = r.Unit
            }).ToList()
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = estimate.Id },
            new ApiResponse<EstimateResponse>(response, null));
    }

    /// <summary>
    /// Get a single estimate by ID with resource breakdown.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EstimateResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var estimate = await _estimateRepository.GetByIdAsync(id, cancellationToken);

        if (estimate is null)
        {
            return NotFound(new ApiResponse<object>(
                null,
                new ApiError(ErrorCodes.NotFound, $"Estimate {id} not found.")));
        }

        var response = new EstimateResponse
        {
            EstimateId = estimate.Id,
            TotalCo2eKg = estimate.TotalCo2eKg,
            CreatedAt = estimate.CreatedAt,
            Breakdown = estimate.Resources.Select(r => new ResourceEstimateResponse
            {
                ResourceType = r.ResourceType,
                Quantity = r.Quantity,
                Hours = r.Hours,
                Region = r.Region,
                Co2eKg = r.Co2eKg,
                Co2ePerUnit = r.Co2ePerUnit,
                Unit = r.Unit
            }).ToList()
        };

        return Ok(new ApiResponse<EstimateResponse>(response, null));
    }

    /// <summary>
    /// List all estimates with pagination, ordered by most recent first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EstimateResponse>>), 200)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var estimates = await _estimateRepository.ListAsync(page, pageSize, cancellationToken);
        var total = await _estimateRepository.CountAsync(cancellationToken);

        var responses = estimates.Select(e => new EstimateResponse
        {
            EstimateId = e.Id,
            TotalCo2eKg = e.TotalCo2eKg,
            CreatedAt = e.CreatedAt,
            Breakdown = e.Resources.Select(r => new ResourceEstimateResponse
            {
                ResourceType = r.ResourceType,
                Quantity = r.Quantity,
                Hours = r.Hours,
                Region = r.Region,
                Co2eKg = r.Co2eKg,
                Co2ePerUnit = r.Co2ePerUnit,
                Unit = r.Unit
            }).ToList()
        }).ToList();

        return Ok(new ApiResponse<List<EstimateResponse>>(
            responses,
            null,
            new ApiMeta(page, total)));
    }
}

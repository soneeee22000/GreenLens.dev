using GreenLens.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GreenLens.Api.Controllers;

/// <summary>
/// Provides runtime configuration for the Angular frontend.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Returns the API key for frontend authentication.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public IActionResult Get()
    {
        return Ok(new ApiResponse<object>(
            Data: new { apiKey = _configuration["API_KEY"] ?? "" },
            Error: null));
    }
}

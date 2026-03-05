using Microsoft.AspNetCore.Mvc;

namespace GreenLens.Api.Controllers;

/// <summary>
/// Health check endpoint for monitoring and load balancer probes.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns service health status.
    /// </summary>
    [HttpGet("/health")]
    [ProducesResponseType(200)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "GreenLens",
            version = "1.0.0",
            timestamp = DateTime.UtcNow
        });
    }
}

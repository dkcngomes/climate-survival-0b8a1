using ClimateAdvisor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClimateAdvisor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendation;

    public RecommendationsController(IRecommendationService recommendation)
    {
        _recommendation = recommendation;
    }

    /// <summary>
    /// Get climate-based purchase recommendations for a given location.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] double lat,
        [FromQuery] double lng,
        CancellationToken ct)
    {
        if (lat < -90 || lat > 90)
            return BadRequest(new { error = "Latitude must be between -90 and 90" });
        if (lng < -180 || lng > 180)
            return BadRequest(new { error = "Longitude must be between -180 and 180" });

        var result = await _recommendation.GenerateRecommendationsAsync(lat, lng, ct);
        return Ok(result);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}

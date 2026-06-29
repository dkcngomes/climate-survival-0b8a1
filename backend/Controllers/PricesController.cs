using ClimateAdvisor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClimateAdvisor.Api.Controllers;

[ApiController]
[Route("api/prices")]
public class PricesController : ControllerBase
{
    private readonly SriLankaPriceService _priceService;

    public PricesController(SriLankaPriceService priceService)
    {
        _priceService = priceService;
    }

    /// <summary>
    /// GET /api/prices/sri-lanka — returns latest CBSL daily price report
    /// for key consumer commodities across 5 Sri Lankan markets.
    /// </summary>
    [HttpGet("sri-lanka")]
    public async Task<IActionResult> GetSriLankaPrices(CancellationToken ct)
    {
        var report = await _priceService.GetLatestPricesAsync(ct);
        if (report == null || report.Commodities.Count == 0)
            return NotFound(new { error = "No price report available. CBSL may not have published a report today." });
        return Ok(report);
    }
}

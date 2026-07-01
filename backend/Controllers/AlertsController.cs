using ClimateAdvisor.Api.Models;
using ClimateAdvisor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClimateAdvisor.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alerts;

    public AlertsController(IAlertService alerts)
    {
        _alerts = alerts;
    }

    /// <summary>Get list of supported carriers for the subscribe form.</summary>
    [HttpGet("carriers")]
    public async Task<ActionResult<List<CarrierEntry>>> GetCarriers()
    {
        var carriers = await _alerts.GetCarriersAsync();
        return Ok(carriers);
    }

    /// <summary>Subscribe to SMS alerts.</summary>
    [HttpPost("subscribe")]
    public async Task<ActionResult<SmsSubscription>> Subscribe([FromBody] SubscribeRequest request)
    {
        if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.CarrierCode))
            return BadRequest(new { error = "Phone number and carrier code are required." });

        if (request.Latitude == 0 && request.Longitude == 0)
            return BadRequest(new { error = "Location coordinates are required." });

        var sub = await _alerts.SubscribeAsync(request);
        return Ok(sub);
    }

    /// <summary>Unsubscribe from SMS alerts.</summary>
    [HttpPost("unsubscribe")]
    public async Task<ActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        var removed = await _alerts.UnsubscribeAsync(request.SubscriptionId);
        if (!removed)
            return NotFound(new { error = "Subscription not found." });
        return Ok(new { success = true });
    }

    /// <summary>Send a test SMS to verify carrier gateway works.</summary>
    [HttpPost("test")]
    public async Task<ActionResult> SendTest([FromBody] TestSmsRequest request)
    {
        if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.CarrierCode))
            return BadRequest(new { error = "Phone number and carrier code are required." });

        var sent = await _alerts.SendTestSmsAsync(request.PhoneNumber, request.CarrierCode);
        if (!sent)
            return BadRequest(new { error = "Unknown carrier code." });

        return Ok(new { success = true, message = "Test SMS sent. Check your phone in a minute." });
    }

    /// <summary>List active subscriptions (for management).</summary>
    [HttpGet("subscriptions")]
    public async Task<ActionResult<List<SmsSubscription>>> GetSubscriptions([FromQuery] string? phone = null)
    {
        var subs = await _alerts.GetSubscriptionsAsync(phone);
        return Ok(subs);
    }
}

using ClimateAdvisor.Api.Models;
using System.Collections.Concurrent;

namespace ClimateAdvisor.Api.Services;

public interface IAlertService
{
    Task<SmsSubscription> SubscribeAsync(SubscribeRequest request, CancellationToken ct = default);
    Task<bool> UnsubscribeAsync(string subscriptionId);
    Task<bool> SendTestSmsAsync(string phoneNumber, string carrierCode, CancellationToken ct = default);
    Task<List<SmsSubscription>> GetSubscriptionsAsync(string? phoneNumber = null);
    Task<List<CarrierEntry>> GetCarriersAsync();
    Task<int> SendClimateAlertAsync(string alertType, string message, double lat, double lng, CancellationToken ct = default);
}

public class AlertService : IAlertService
{
    private readonly IEmailService _email;
    private readonly ILogger<AlertService> _logger;

    // In-memory subscription store (for MVP; swap to DB later)
    private static readonly ConcurrentDictionary<string, SmsSubscription> Subscriptions = new();

    public AlertService(IEmailService email, ILogger<AlertService> logger)
    {
        _email = email;
        _logger = logger;
    }

    public Task<List<CarrierEntry>> GetCarriersAsync()
    {
        return Task.FromResult(CarrierInfo.All);
    }

    public async Task<SmsSubscription> SubscribeAsync(SubscribeRequest request, CancellationToken ct = default)
    {
        var sub = new SmsSubscription
        {
            PhoneNumber = new string(request.PhoneNumber.Where(char.IsDigit).ToArray()),
            CarrierCode = request.CarrierCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationName = request.LocationName,
            CountryCode = request.CountryCode,
            AlertTypes = request.AlertTypes ?? "drought,flood,storm,heatwave,planting",
        };

        Subscriptions[sub.Id] = sub;

        // Send a welcome/test SMS
        var gatewayEmail = CarrierInfo.ToSmsEmail(sub.PhoneNumber, sub.CarrierCode);
        if (gatewayEmail != null)
        {
            await _email.SendRawAsync(
                gatewayEmail,
                "Climate Survival: Alert Active",
                $"✅ You're subscribed!\n\nLocation: {request.LocationName ?? "your area"}\nAlerts: {sub.AlertTypes}\n\nYou'll get SMS when climate conditions change.\n\n— Climate Survival",
                ct
            );
        }

        _logger.LogInformation("New SMS subscription: {Id} → {Phone} ({Carrier})", sub.Id, sub.PhoneNumber, sub.CarrierCode);
        return sub;
    }

    public Task<bool> UnsubscribeAsync(string subscriptionId)
    {
        var removed = Subscriptions.TryRemove(subscriptionId, out _);
        _logger.LogInformation("SMS unsubscribed: {Id} (removed: {Removed})", subscriptionId, removed);
        return Task.FromResult(removed);
    }

    public Task<List<SmsSubscription>> GetSubscriptionsAsync(string? phoneNumber = null)
    {
        var all = Subscriptions.Values
            .Where(s => s.IsActive)
            .AsEnumerable();

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            all = all.Where(s => s.PhoneNumber == digits);
        }

        return Task.FromResult(all.OrderByDescending(s => s.SubscribedAt).ToList());
    }

    public async Task<bool> SendTestSmsAsync(string phoneNumber, string carrierCode, CancellationToken ct = default)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        var gatewayEmail = CarrierInfo.ToSmsEmail(digits, carrierCode);

        if (gatewayEmail == null)
        {
            _logger.LogWarning("Unknown carrier code: {Carrier}", carrierCode);
            return false;
        }

        await _email.SendRawAsync(
            gatewayEmail,
            "Climate Survival: Test Alert",
            $"🔔 This is a test SMS from Climate Survival.\n\nYour carrier: {carrierCode}\nTime: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n\nReply UNSUBSCRIBE anytime to stop alerts.\n\n— Climate Survival",
            ct
        );

        return true;
    }

    public async Task<int> SendClimateAlertAsync(string alertType, string message, double lat, double lng, CancellationToken ct = default)
    {
        var affected = Subscriptions.Values
            .Where(s => s.IsActive)
            .Where(s => Math.Abs(s.Latitude - lat) < 1.0 && Math.Abs(s.Longitude - lng) < 1.0)
            .Where(s => s.AlertTypes.Split(',').Contains(alertType, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (affected.Count == 0) return 0;

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var subject = $"⚠️ Climate Alert: {alertType}";
        var body = $"⚠️ CLIMATE ALERT — {alertType.ToUpperInvariant()}\n\n{message}\n\n— Climate Survival ({timestamp})";

        var sent = 0;
        foreach (var sub in affected)
        {
            var gatewayEmail = CarrierInfo.ToSmsEmail(sub.PhoneNumber, sub.CarrierCode);
            if (gatewayEmail == null) continue;

            await _email.SendRawAsync(gatewayEmail, subject, body, ct);
            sub.LastAlertSentAt = DateTime.UtcNow;
            sub.AlertCount++;
            sent++;
        }

        _logger.LogInformation("Sent {Sent}/{Total} climate alerts of type '{Type}' near ({Lat}, {Lng})",
            sent, affected.Count, alertType, lat, lng);
        return sent;
    }
}

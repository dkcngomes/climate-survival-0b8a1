namespace ClimateAdvisor.Api.Models;

/// <summary>A user's SMS alert subscription.</summary>
public class SmsSubscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string PhoneNumber { get; set; } = string.Empty;
    public string CarrierCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationName { get; set; }
    public string? CountryCode { get; set; }
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? LastAlertSentAt { get; set; }
    public int AlertCount { get; set; }

    /// <summary>Comma-separated alert types: drought, flood, storm, heatwave, coldspell, planting</summary>
    public string AlertTypes { get; set; } = "drought,flood,storm,heatwave,planting";
}

/// <summary>Request model for subscribing.</summary>
public class SubscribeRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string CarrierCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationName { get; set; }
    public string? CountryCode { get; set; }
    public string? AlertTypes { get; set; }
}

/// <summary>Request model for unsubscribing.</summary>
public class UnsubscribeRequest
{
    public string SubscriptionId { get; set; } = string.Empty;
}

/// <summary>Request model for sending a test SMS.</summary>
public class TestSmsRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string CarrierCode { get; set; } = string.Empty;
}

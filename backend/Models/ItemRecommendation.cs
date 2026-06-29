namespace ClimateAdvisor.Api.Models;

public class ItemRecommendation
{
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string RiskLevel { get; set; } = "Medium";
    public string SuggestedAction { get; set; } = string.Empty;
    public string StorageTip { get; set; } = string.Empty;
    public ClimateSignal TriggerSignal { get; set; }
}

public class RecommendationResponse
{
    public ClimateForecast Forecast { get; set; } = new();
    public List<ItemRecommendation> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string OverallRiskLevel { get; set; } = "Low";
}

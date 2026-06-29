using System.Text.Json;

namespace ClimateAdvisor.Api.Services;

/// <summary>
/// Fetches live exchange rates using the Frankfurter API (free, no API key required).
/// https://www.frankfurter.app/
/// </summary>
public class ExchangeRateService
{
    private readonly HttpClient _http;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly Dictionary<string, decimal> _cachedRates = new();
    private DateTime _lastFetch = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6); // Refresh twice daily

    private const string FrankfurterApi = "https://api.frankfurter.app/latest?from=USD";

    public ExchangeRateService(HttpClient http, ILogger<ExchangeRateService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Get the exchange rate from USD to the target currency.
    /// Returns 1.0m if the rate can't be fetched (falls back to USD).
    /// </summary>
    public async Task<decimal> GetRateAsync(string toCurrency, CancellationToken ct = default)
    {
        if (toCurrency == "USD") return 1.0m;

        if (_cachedRates.TryGetValue(toCurrency, out var cached) &&
            DateTime.UtcNow - _lastFetch < CacheDuration)
            return cached;

        try
        {
            var url = $"{FrankfurterApi}&to={toCurrency}";
            var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Frankfurter API returned {Status} for {Currency}", response.StatusCode, toCurrency);
                return 1.0m;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var rates = doc.RootElement.GetProperty("rates");

            if (rates.TryGetProperty(toCurrency, out var rateEl) &&
                rateEl.TryGetDecimal(out var rate))
            {
                _cachedRates[toCurrency] = rate;
                _lastFetch = DateTime.UtcNow;
                _logger.LogInformation("Exchange rate USD → {Currency}: {Rate}", toCurrency, rate);
                return rate;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch exchange rate for {Currency}", toCurrency);
        }

        return 1.0m; // Fallback to USD
    }

    /// <summary>
    /// Get base prices (in USD) for common stock-up items.
    /// </summary>
    public static decimal GetBasePriceUsd(string itemName)
    {
        return itemName.ToLowerInvariant() switch
        {
            // Grains
            "rice" => 1.50m,
            "flour" => 1.20m,
            "pasta" => 1.80m,
            "bread" => 2.50m,
            // Canned
            "canned food" => 2.00m,
            "canned beans" => 1.80m,
            "canned vegetables" => 1.50m,
            "canned soup" => 2.50m,
            "canned meat" => 3.50m,
            "canned fish" => 2.80m,
            "canned tuna" => 2.50m,
            "canned tomatoes" => 1.50m,
            // Oils
            "cooking oil" => 5.00m,
            "olive oil" => 8.00m,
            "coconut oil" => 4.50m,
            // Protein
            "beef" => 6.00m,
            "chicken" => 4.00m,
            "pork" => 4.50m,
            "eggs" => 3.00m,
            "milk" => 3.50m,
            "powdered milk" => 5.00m,
            "cheese" => 5.50m,
            // Essentials
            "bottled water" => 1.00m,
            "sugar" => 2.00m,
            "salt" => 1.00m,
            "batteries" => 5.00m,
            "toilet paper" => 6.00m,
            "soap" => 2.50m,
            "medicine" => 10.00m,
            "first aid kit" => 15.00m,
            // Beverages
            "coffee" => 8.00m,
            "tea" => 4.00m,
            "juice" => 3.50m,
            // Other
            "charcoal" => 5.00m,
            "firewood" => 8.00m,
            "torch" => 10.00m,
            "flashlight" => 12.00m,
            _ => 3.00m, // Default
        };
    }
}

using System.Text.Json.Serialization;
using ClimateAdvisor.Api.Models;

namespace ClimateAdvisor.Api.Services;

public class ClimateService : IClimateService
{
    private readonly HttpClient _http;
    private readonly IHydroMeteoService _hydroMeteo;

    public ClimateService(HttpClient http, IHydroMeteoService hydroMeteo)
    {
        _http = http;
        _hydroMeteo = hydroMeteo;
    }

    public async Task<ClimateForecast> GetForecastAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        var forecast = new ClimateForecast
        {
            Latitude = latitude,
            Longitude = longitude,
            ForecastDate = DateTime.UtcNow
        };

        // 1. Reverse geocode
        await ResolveLocationAsync(forecast, ct);

        // 2. Seasonal forecast anomalies (long-term climate — 3-month outlook)
        await FetchSeasonalAnomaliesAsync(forecast, ct);
        forecast.ForecastPeriodLabel = "3-Month Seasonal Climate Outlook + 7-Day Weather Forecast";

        // 3. Hydrometeorological data (short-term: soil moisture, flood risk, etc.)
        var hydroData = await _hydroMeteo.GetHydroMeteoDataAsync(latitude, longitude, ct);
        forecast.HydroMeteo = hydroData;

        // 4. Detect all signals
        forecast.DetectedSignals = DetectSignals(forecast);

        return forecast;
    }

    private async Task ResolveLocationAsync(ClimateForecast forecast, CancellationToken ct)
    {
        try
        {
            var url = $"https://api.bigdatacloud.net/data/reverse-geocode-client" +
                      $"?latitude={forecast.Latitude}&longitude={forecast.Longitude}&localityLanguage=en";
            var response = await _http.GetFromJsonAsync<ReverseGeoResponse>(url, ct);

            if (response != null)
            {
                var parts = new[] { response.City, response.Locality, response.PrincipalSubdivision, response.CountryName }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!)
                    .Distinct()
                    .ToList();

                forecast.LocationName = parts.Count > 0 ? parts[0] : $"({forecast.Latitude:F2}, {forecast.Longitude:F2})";
                forecast.Region = parts.Count > 1 ? string.Join(", ", parts.Skip(1)) : "Unknown";
            }
        }
        catch
        {
            forecast.LocationName = $"({forecast.Latitude:F2}, {forecast.Longitude:F2})";
            forecast.Region = "Unknown";
        }
    }

    private async Task FetchSeasonalAnomaliesAsync(ClimateForecast forecast, CancellationToken ct)
    {
        try
        {
            var url = $"https://seasonal-api.open-meteo.com/v1/seasonal?" +
                      $"latitude={forecast.Latitude}&longitude={forecast.Longitude}" +
                      $"&daily=temperature_2m_mean,precipitation_sum" +
                      $"&weekly=temperature_2m_anomaly,precipitation_anomaly,temperature_2m_efi,precipitation_efi" +
                      $"&timezone=auto" +
                      $"&forecast_months=3";

            var response = await _http.GetFromJsonAsync<SeasonalApiResponse>(url, ct);

            if (response?.Weekly != null)
            {
                var temps = response.Weekly.TemperatureAnomaly?.Where(v => v.HasValue).Select(v => v!.Value).ToList();
                var preps = response.Weekly.PrecipitationAnomaly?.Where(v => v.HasValue).Select(v => v!.Value).ToList();
                var efiTemps = response.Weekly.ExtremeForecastIndexTemperature?.Where(v => v.HasValue).Select(v => v!.Value).ToList();
                var efiPreps = response.Weekly.ExtremeForecastIndexPrecipitation?.Where(v => v.HasValue).Select(v => v!.Value).ToList();

                if (temps?.Count > 0) forecast.TemperatureAnomaly = Math.Round(temps.Average(), 2);
                if (preps?.Count > 0) forecast.PrecipitationAnomaly = Math.Round(preps.Average(), 2);
                if (efiTemps?.Count > 0) forecast.ExtremeTemperatureIndex = Math.Round(efiTemps.Average(), 2);
                if (efiPreps?.Count > 0) forecast.ExtremePrecipitationIndex = Math.Round(efiPreps.Average(), 2);

                forecast.Probability = 70;
            }
        }
        catch
        {
            forecast.TemperatureAnomaly = 0;
            forecast.PrecipitationAnomaly = 0;
            forecast.ExtremeTemperatureIndex = 0;
            forecast.ExtremePrecipitationIndex = 0;
            forecast.Probability = 50;
        }
    }

    private List<ClimateSignal> DetectSignals(ClimateForecast f)
    {
        var signals = new List<ClimateSignal>();

        // ── Seasonal climate signals ──
        if (f.TemperatureAnomaly > 1.5 && f.PrecipitationAnomaly < -20)
            signals.Add(ClimateSignal.ElNino);
        else if (f.TemperatureAnomaly < -1.0 && f.PrecipitationAnomaly > 30)
            signals.Add(ClimateSignal.LaNina);
        else if (f.PrecipitationAnomaly < -30)
            signals.Add(ClimateSignal.Drought);
        else if (f.PrecipitationAnomaly > 40)
            signals.Add(ClimateSignal.HeavyRainfall);
        else if (f.ExtremeTemperatureIndex > 0.7)
            signals.Add(ClimateSignal.Heatwave);
        else if (f.TemperatureAnomaly < -3.0)
            signals.Add(ClimateSignal.ColdSpell);

        // ── Hydrometeorological signals (short-term) ──
        if (f.HydroMeteo != null)
        {
            if (f.HydroMeteo.FloodRiskIndex > 50)
                signals.Add(ClimateSignal.FloodRisk);

            if (f.HydroMeteo.StormSeverityIndex > 50)
                signals.Add(ClimateSignal.StormRisk);

            if (f.HydroMeteo.DroughtSeverityIndex > 60)
                signals.Add(ClimateSignal.ExtremeDrought);
        }

        if (signals.Count == 0)
            signals.Add(ClimateSignal.Normal);

        return signals;
    }

    // ── DTOs ──
    private record ReverseGeoResponse(string? City, string? Locality, string? PrincipalSubdivision, string? CountryName);

    private record SeasonalApiResponse(
        [property: JsonPropertyName("daily")] SeasonalDaily? Daily,
        [property: JsonPropertyName("weekly")] SeasonalWeekly? Weekly
    );
    private record SeasonalDaily(
        [property: JsonPropertyName("temperature_2m_mean")] double?[]? TemperatureMean,
        [property: JsonPropertyName("precipitation_sum")] double?[]? PrecipitationSum
    );
    private record SeasonalWeekly(
        [property: JsonPropertyName("temperature_2m_anomaly")] double?[]? TemperatureAnomaly,
        [property: JsonPropertyName("precipitation_anomaly")] double?[]? PrecipitationAnomaly,
        [property: JsonPropertyName("temperature_2m_efi")] double?[]? ExtremeForecastIndexTemperature,
        [property: JsonPropertyName("precipitation_efi")] double?[]? ExtremeForecastIndexPrecipitation
    );
}

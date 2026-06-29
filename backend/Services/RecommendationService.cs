using ClimateAdvisor.Api.Models;

namespace ClimateAdvisor.Api.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IClimateService _climate;
    private readonly IPriceService _prices;
    private readonly ExchangeRateService _exchangeRates;
    private readonly ICountryLocaleService _locales;

    public RecommendationService(
        IClimateService climate,
        IPriceService prices,
        ExchangeRateService exchangeRates,
        ICountryLocaleService locales)
    {
        _climate = climate;
        _prices = prices;
        _exchangeRates = exchangeRates;
        _locales = locales;
    }

    public async Task<RecommendationResponse> GenerateRecommendationsAsync(
        double latitude, double longitude, string? currencyCode = null, CancellationToken ct = default)
    {
        var forecast = await _climate.GetForecastAsync(latitude, longitude, ct);
        var priceData = await _prices.GetPricesAsync(ct);
        var allRules = ClimateRules.GetAll();

        var triggeredSignals = forecast.DetectedSignals.ToHashSet();
        var matchedRules = allRules
            .Where(r => triggeredSignals.Contains(r.TriggerSignal))
            .OrderBy(r => r.Priority)
            .ToList();

        // ── Determine target currency ──
        if (string.IsNullOrEmpty(currencyCode))
            currencyCode = "USD";

        // Get currency metadata from locale service if possible
        string currencySymbol = "$";
        try
        {
            // Find a locale that uses this currency code
            var allLocales = _locales.GetAllLocales();
            var match = allLocales.FirstOrDefault(l =>
                l.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                currencySymbol = match.CurrencySymbol;
        }
        catch { /* fallback */ }

        // ── Get exchange rate ──
        var rate = await _exchangeRates.GetRateAsync(currencyCode, ct);

        var recommendations = new List<ItemRecommendation>();

        foreach (var rule in matchedRules)
        {
            foreach (var item in rule.Items)
            {
                // Calculate local price from USD base
                var baseUsd = ExchangeRateService.GetBasePriceUsd(item.ItemName);
                var localPrice = Math.Round(baseUsd * rate, 2);

                recommendations.Add(new ItemRecommendation
                {
                    ItemName = item.ItemName,
                    Category = item.Category,
                    Reason = item.Reason,
                    Priority = rule.Priority,
                    RiskLevel = rule.RiskLevel,
                    SuggestedAction = item.SuggestedAction,
                    StorageTip = item.StorageTip,
                    TriggerSignal = rule.TriggerSignal,
                    EstimatedPrice = localPrice,
                    CurrencyCode = currencyCode,
                    CurrencySymbol = currencySymbol,
                });
            }
        }

        // Determine overall risk
        var overallRisk = recommendations.Count > 0
            ? recommendations.Max(r => r.RiskLevel switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                _ => 1
            }) switch
            {
                4 => "Critical",
                3 => "High",
                2 => "Medium",
                _ => "Low"
            }
            : "Low";

        return new RecommendationResponse
        {
            Forecast = forecast,
            Recommendations = recommendations,
            GeneratedAt = DateTime.UtcNow,
            OverallRiskLevel = overallRisk
        };
    }
}

/// <summary>
/// Rule definitions mapping climate signals to consumer item recommendations.
/// </summary>
public static class ClimateRules
{
    public static List<ClimateRule> GetAll() => new()
    {
        // ===== EL NIÑO =====
        new()
        {
            TriggerSignal = ClimateSignal.ElNino,
            Priority = 1,
            RiskLevel = "High",
            Items = new()
            {
                new() { ItemName = "Rice", Category = "Grains", Reason = "El Niño disrupts Asian rice production → prices typically rise 15-30% within 3 months", SuggestedAction = "Buy 2-3 month supply now before prices spike", StorageTip = "Store in airtight container in cool, dry place" },
                new() { ItemName = "Flour / Wheat", Category = "Grains", Reason = "Global wheat yields affected by El Niño weather patterns", SuggestedAction = "Stock extra 2 months of flour supply", StorageTip = "Keep in sealed container, use within 6 months" },
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "El Niño → food inflation expected, canned goods are price-stable alternatives", SuggestedAction = "Build a 2-week emergency supply", StorageTip = "Check expiry dates, rotate stock" },
                new() { ItemName = "Cooking Oil", Category = "Oils & Fats", Reason = "Soybean and palm oil yields drop in El Niño conditions", SuggestedAction = "Buy extra 2-3 bottles before price hikes", StorageTip = "Store in dark, cool cabinet away from heat" },
                new() { ItemName = "Sugar", Category = "Food", Reason = "Sugar cane production impacted by El Niño drought conditions", SuggestedAction = "Stock extra supply if you bake regularly", StorageTip = "Airtight container in dry pantry" },
                new() { ItemName = "Powdered Milk", Category = "Canned & Preserved", Reason = "Dairy production costs rise with feed prices during El Niño", SuggestedAction = "Consider buying extra powdered milk for pantry", StorageTip = "Sealed, cool, dark place — lasts 12-18 months" },
            }
        },

        // ===== LA NIÑA =====
        new()
        {
            TriggerSignal = ClimateSignal.LaNina,
            Priority = 2,
            RiskLevel = "High",
            Items = new()
            {
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "La Niña floods disrupt supply chains and transport routes", SuggestedAction = "Stock 2+ weeks of non-perishable food", StorageTip = "Keep in cool, dry pantry. Rotate every 6 months." },
                new() { ItemName = "Bottled Water", Category = "Beverages", Reason = "Heavy rainfall can flood water treatment plants → shortages possible", SuggestedAction = "Store 5-10 gallons per person for emergencies", StorageTip = "Replace every 6 months, store in dark cool place" },
                new() { ItemName = "Rice", Category = "Grains", Reason = "Excess rain damages standing crops and delays harvests", SuggestedAction = "Buy 1-2 months extra supply", StorageTip = "Airtight container with oxygen absorber for long term" },
                new() { ItemName = "Batteries", Category = "Essentials", Reason = "Flooding increases risk of power outages", SuggestedAction = "Stock AA, AAA, and power bank batteries", StorageTip = "Store at room temperature, check expiry dates" },
            }
        },

        // ===== DROUGHT =====
        new()
        {
            TriggerSignal = ClimateSignal.Drought,
            Priority = 1,
            RiskLevel = "Critical",
            Items = new()
            {
                new() { ItemName = "Rice", Category = "Grains", Reason = "Drought severely impacts rice paddies → major price surge expected", SuggestedAction = "Prioritize rice purchase — 3+ month supply recommended", StorageTip = "Use food-grade buckets with gamma seal lids" },
                new() { ItemName = "Flour / Wheat", Category = "Grains", Reason = "Wheat yields drastically reduced in drought conditions", SuggestedAction = "Stock 3 months of flour", StorageTip = "Freeze for 48h first to kill bugs, then airtight container" },
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "Fresh produce becomes expensive; canned offers stable alternative", SuggestedAction = "Build 4-week emergency pantry", StorageTip = "Rotate stock — first in, first out" },
                new() { ItemName = "Cooking Oil", Category = "Oils & Fats", Reason = "Oilseed crops fail in drought → cooking oil prices surge", SuggestedAction = "Buy 3-4 bottles while prices are normal", StorageTip = "Cool dark place, use within 6 months of opening" },
                new() { ItemName = "Powdered Milk", Category = "Canned & Preserved", Reason = "Dairy production drops sharply during drought conditions", SuggestedAction = "Stock extra powdered milk for 2 months", StorageTip = "Sealed, below 25°C, use within 12 months" },
                new() { ItemName = "Beef / Meat", Category = "Protein", Reason = "Drought forces herd reduction → beef prices rise significantly", SuggestedAction = "Buy in bulk and freeze if possible", StorageTip = "Vacuum seal and freeze — lasts 6-12 months" },
            }
        },

        // ===== HEAVY RAINFALL / FLOOD =====
        new()
        {
            TriggerSignal = ClimateSignal.HeavyRainfall,
            Priority = 2,
            RiskLevel = "High",
            Items = new()
            {
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "Flooding disrupts food supply chains immediately", SuggestedAction = "Stock minimum 1-week supply of ready-to-eat canned food", StorageTip = "Keep in elevated storage to avoid flood damage" },
                new() { ItemName = "Bottled Water", Category = "Beverages", Reason = "Flooding contaminates freshwater supplies", SuggestedAction = "Store 3+ gallons per person", StorageTip = "Replace stored water every 6 months" },
                new() { ItemName = "Batteries", Category = "Essentials", Reason = "Power outages common during severe storms and flooding", SuggestedAction = "Buy assorted batteries and power banks", StorageTip = "Store in waterproof container" },
                new() { ItemName = "Chicken / Poultry", Category = "Protein", Reason = "Poultry farms flood → supply drops and prices rise", SuggestedAction = "Buy extra chicken, freeze in portions", StorageTip = "Freeze at 0°F (-18°C), use within 6 months" },
            }
        },

        // ===== HEATWAVE =====
        new()
        {
            TriggerSignal = ClimateSignal.Heatwave,
            Priority = 3,
            RiskLevel = "Medium",
            Items = new()
            {
                new() { ItemName = "Bottled Water", Category = "Beverages", Reason = "Heatwaves increase water demand and can cause shortages", SuggestedAction = "Store extra water and electrolyte drinks", StorageTip = "Keep in shaded area, replace every 6 months" },
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "Avoid cooking during extreme heat — canned foods need no preparation", SuggestedAction = "Stock no-cook meal options", StorageTip = "Standard pantry storage" },
            }
        },

        // ===== COLD SPELL =====
        new()
        {
            TriggerSignal = ClimateSignal.ColdSpell,
            Priority = 3,
            RiskLevel = "Medium",
            Items = new()
            {
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "Heating costs spike — minimize cooking with prepared canned goods", SuggestedAction = "Stock soup and ready-to-heat canned items", StorageTip = "Standard pantry storage" },
                new() { ItemName = "Powdered Milk", Category = "Canned & Preserved", Reason = "Dairy supply constricted by cold weather affecting transport", SuggestedAction = "Keep extra powdered milk on hand", StorageTip = "Cool pantry, away from temperature swings" },
            }
        },

        // ===== FLOOD RISK (hydrometeorological) =====
        new()
        {
            TriggerSignal = ClimateSignal.FloodRisk,
            Priority = 1,
            RiskLevel = "Critical",
            Items = new()
            {
                new() { ItemName = "Bottled Water", Category = "Beverages", Reason = "Active flood risk detected — water contamination imminent. River discharge is critically high and soil is saturated (HydroMeteo: flood index)", SuggestedAction = "STORE 5+ GALLONS PER PERSON IMMEDIATELY", StorageTip = "Store in sealed containers on elevated floor" },
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "Supply chain disruption expected within 48-72h due to flooding", SuggestedAction = "Buy 2+ weeks of ready-to-eat canned food now", StorageTip = "Keep in waterproof bins, elevated above potential flood level" },
                new() { ItemName = "Batteries", Category = "Essentials", Reason = "Flooding causes power outages — be prepared for 3-5 days without electricity", SuggestedAction = "Stock AA, AAA batteries and portable power banks", StorageTip = "Store in waterproof container with emergency kit" },
                new() { ItemName = "First Aid Kit", Category = "Health", Reason = "Flooding increases injury risk and reduces access to medical facilities", SuggestedAction = "Ensure first aid kit is fully stocked", StorageTip = "Keep in accessible, waterproof bag" },
                new() { ItemName = "Rice", Category = "Grains", Reason = "Road closures from flooding will disrupt food deliveries", SuggestedAction = "Buy 2-3 weeks of rice supply while roads are open", StorageTip = "Store in sealed buckets with oxygen absorbers" },
            }
        },

        // ===== STORM RISK (hydrometeorological) =====
        new()
        {
            TriggerSignal = ClimateSignal.StormRisk,
            Priority = 1,
            RiskLevel = "High",
            Items = new()
            {
                new() { ItemName = "Batteries", Category = "Essentials", Reason = "High wind gusts (>{MaxWindGust} km/h detected) → power outages expected", SuggestedAction = "Stock flashlights, batteries, and power banks immediately", StorageTip = "Store in an accessible emergency kit near shelter area" },
                new() { ItemName = "Bottled Water", Category = "Beverages", Reason = "Storm damage to water infrastructure may cause service interruptions", SuggestedAction = "Store 3+ gallons per person for 3 days", StorageTip = "Fill clean containers before storm arrives" },
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "Power outages mean refrigeration loss — stock non-perishable food", SuggestedAction = "Buy 1 week of no-cook or ready-to-eat food", StorageTip = "Store away from windows in case of glass breakage" },
                new() { ItemName = "Duct Tape & Tarps", Category = "Home", Reason = "Storm winds may damage roofs and windows", SuggestedAction = "Buy emergency repair supplies for temporary patching", StorageTip = "Store with emergency kit in accessible location" },
                new() { ItemName = "Chicken / Poultry", Category = "Protein", Reason = "Freeze extra protein before storm — power loss will thaw frozen food", SuggestedAction = "If freezer is full, cook and can extra meat", StorageTip = "Keep freezer at max cold setting before storm" },
            }
        },

        // ===== EXTREME DROUGHT (hydrometeorological) =====
        new()
        {
            TriggerSignal = ClimateSignal.ExtremeDrought,
            Priority = 1,
            RiskLevel = "Critical",
            Items = new()
            {
                new() { ItemName = "Bottled Water", Category = "Beverages", Reason = "Extreme drought detected — soil moisture critically low, water restrictions imminent (HydroMeteo: drought severity {DroughtSeverityIndex}/100)", SuggestedAction = "STORE 10+ GALLONS PER PERSON BEFORE WATER RESTRICTIONS BEGIN", StorageTip = "Store in cool dark place. Replace every 6 months." },
                new() { ItemName = "Rice", Category = "Grains", Reason = "Agricultural collapse imminent — rice production will drop 40-60%", SuggestedAction = "BUY 3-6 MONTH SUPPLY IMMEDIATELY — prices will surge", StorageTip = "Long-term storage: mylar bags with oxygen absorbers in buckets" },
                new() { ItemName = "Flour / Wheat", Category = "Grains", Reason = "Wheat crop failure expected — global supply chain impact", SuggestedAction = "Stock minimum 3 months of flour and baking supplies", StorageTip = "Freeze for 48h first, then airtight containers" },
                new() { ItemName = "Canned Food (Mixed)", Category = "Canned & Preserved", Reason = "Fresh produce and meat will become scarce and expensive", SuggestedAction = "Build 8-week emergency pantry", StorageTip = "Rotate stock. Buy extra vegetables and fruits." },
                new() { ItemName = "Cooking Oil", Category = "Oils & Fats", Reason = "Oilseed crops devastated by drought conditions", SuggestedAction = "Buy 4-5 bottles while prices are still normal", StorageTip = "Cool dark cabinet. Consider freezing for longer storage." },
                new() { ItemName = "Powdered Milk", Category = "Canned & Preserved", Reason = "Dairy industry collapses during extreme drought — feed and water shortages", SuggestedAction = "Stock 3+ months of powdered milk", StorageTip = "Sealed, below 25°C. Use within 12-18 months." },
                new() { ItemName = "Beef / Meat", Category = "Protein", Reason = "Massive herd culling expected — meat prices will spike 100%+", SuggestedAction = "Buy bulk meat now and deep freeze", StorageTip = "Vacuum seal and freeze. Good for 12 months." },
            }
        },
    };
}

public class ClimateRule
{
    public ClimateSignal TriggerSignal { get; set; }
    public int Priority { get; set; } // 1 = most urgent
    public string RiskLevel { get; set; } = "Medium";
    public List<RuleItem> Items { get; set; } = new();
}

public class RuleItem
{
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string SuggestedAction { get; set; } = string.Empty;
    public string StorageTip { get; set; } = string.Empty;
}

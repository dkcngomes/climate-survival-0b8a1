using ClimateAdvisor.Api.Models;
using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace ClimateAdvisor.Api.Services;

public interface ICropRecommendationService
{
    Task<CropRecommendationResponse> GetCropRecommendationsAsync(ClimateForecast forecast, string? countryCode = null, CancellationToken ct = default);
}

public class CropRecommendationService : ICropRecommendationService
{
    private readonly HttpClient _http;
    private readonly IGeminiService _gemini;
    private readonly ILogger<CropRecommendationService> _logger;
    private static readonly ConcurrentDictionary<string, (string? ImageUrl, string? WikiUrl)> ImageCache = new();

    public CropRecommendationService(HttpClient http, IGeminiService gemini, ILogger<CropRecommendationService> logger)
    {
        _http = http;
        _gemini = gemini;
        _logger = logger;
    }

    private static readonly List<CropEntry> CropCatalog = new()
    {
        // ===== HEAT & DROUGHT TOLERANT =====
        new("Sweet Potato", "Root Vegetables",
            "Highly nutritious, heat and drought tolerant storage root. Thrives where other crops fail.",
            90, 120, "High", "Low", "High", "Low",
            "Excellent for hot, dry conditions — stores well for months after harvest.",
            "Plant slips in mounds or raised beds. Use mulch to retain moisture.",
            "Slips (sprouted cuttings)"),
        new("Okra", "Vegetables",
            "Thrives in intense heat. Produces continuously through hot summers.",
            50, 60, "High", "Low", "High", "Low",
            "One of the most heat-tolerant vegetables — keeps producing when others bolt.",
            "Soak seeds 24h before planting. Harvest pods at 5-8cm for best tenderness.",
            "Direct sow after soil warms"),
        new("Amaranth (Grain & Leaves)", "Grains",
            "Extremely heat-tolerant pseudo-grain. Leaves edible too. Survives poor soil.",
            30, 90, "High", "Low", "High", "Medium",
            "Leaves in 30 days, grain in 90. Nearly impossible to kill in heat.",
            "Thin to 30cm apart. Pinch leaves for continual harvest.",
            "Direct sow shallow, 5mm deep"),
        new("Sorghum", "Grains",
            "Drought-resistant grain crop. Staple food in arid regions. Heat loving.",
            90, 120, "High", "Low", "High", "Medium",
            "Survives where corn fails — deep root system accesses underground moisture.",
            "Plant after frost. Requires little water once established.",
            "Direct sow, 2-3cm deep in rows"),
        new("Millet", "Grains",
            "Fast-growing drought-tolerant grain. Grows in poor soil with minimal water.",
            60, 90, "High", "Low", "High", "Medium",
            "Grows in extreme heat with very little water — perfect drought insurance crop.",
            "Broadcast sow densely. Can be used as green manure too.",
            "Broadcast or row planted"),
        new("Cassava", "Root Vegetables",
            "Extremely drought-tolerant starchy root. Survivor crop in harsh conditions.",
            300, 365, "High", "Low", "High", "Low",
            "The ultimate survival crop — produces even in drought. Stays in ground for months.",
            "Plant stem cuttings at 45° angle. Harvest after leaves yellow.",
            "Stem cuttings (20-30cm)"),
        new("Cowpeas", "Legumes",
            "Heat-loving legume that fixes nitrogen. Both pods and leaves edible.",
            60, 90, "High", "Low", "High", "Medium",
            "Thrives in hot humid conditions. Fixes nitrogen for future crops too.",
            "Sow after frost. Southern peas need heat to set pods.",
            "Direct sow, 2-3cm deep"),
        new("Chickpeas", "Legumes",
            "Drought-tolerant protein-rich legume. Prefers dry conditions.",
            90, 110, "Medium", "Medium", "High", "Low",
            "Excellent dryland crop — deep roots tap moisture other crops can't reach.",
            "Plant in cool weather. No additional watering after establishment.",
            "Direct sow in well-drained soil"),
        new("Lentils", "Legumes",
            "Low-water legume rich in protein. Quick to harvest.",
            80, 110, "Medium", "High", "High", "Low",
            "Very low water needs. Grows in cool dry conditions too.",
            "Sow early spring. Harvest when lower pods turn brown.",
            "Direct sow, shallow 2cm"),
        new("Pigeon Pea", "Legumes",
            "Woody perennial legume. Survives drought, poor soil, and heat.",
            150, 180, "High", "Low", "High", "Medium",
            "Deep roots make it exceptionally drought-hardy. Produces for 3-5 years.",
            "Prune to encourage bushier growth. Harvest pods repeatedly.",
            "Direct sow or seedling"),
        new("Sesame", "Grains",
            "Heat-loving oil seed. Very drought tolerant once established.",
            90, 120, "High", "Low", "High", "Low",
            "Ancient drought-resistant oil crop. Seeds store for years.",
            "Sow after soil thoroughly warm. Harvest when lower seed pods split.",
            "Direct sow fine seeds shallow"),
        new("Quinoa", "Grains",
            "Drought-tolerant protein-rich pseudograin. Adapted to harsh conditions.",
            90, 120, "Medium", "Medium", "High", "Low",
            "Tolerates cold nights and dry days. Complete protein profile.",
            "Rinse seeds before planting to remove saponins.",
            "Direct sow after frost"),
        new("Eggplant", "Vegetables",
            "Heat-loving vegetable. Grows well through hot, humid summers.",
            70, 85, "High", "Low", "Medium", "Low",
            "Fruits set best in hot weather — perfect warming-climate crop.",
            "Stake plants. Harvest fruits while skin is glossy.",
            "Seedlings or direct sow warm"),

        // ===== COOL WEATHER / COLD TOLERANT =====
        new("Kale", "Leafy Greens",
            "Extremely cold-hardy. Frost improves sweetness. Survives winter.",
            50, 75, "Low", "High", "Medium", "Medium",
            "Hardiest leafy green — survives freezing temps. Frost makes leaves sweeter.",
            "Harvest outer leaves first. Plant in succession for continuous supply.",
            "Direct sow or seedlings"),
        new("Spinach", "Leafy Greens",
            "Fast-growing cool weather green. Quick harvest in cold conditions.",
            30, 45, "Low", "High", "Medium", "Medium",
            "Quickest cool-season crop. Tolerates light frost.",
            "Sow every 2 weeks for continuous harvest. Bolts in heat.",
            "Direct sow, 1-2cm deep"),
        new("Carrots", "Root Vegetables",
            "Cool-weather root crop. Stores well long-term.",
            50, 80, "Low", "High", "Medium", "Low",
            "Grows well in cold soil. Stores for months in damp sand.",
            "Loose deep soil needed. Thin to 5cm apart.",
            "Direct sow shallow, 0.5cm"),
        new("Potatoes", "Root Vegetables",
            "Staple cool-weather crop. High calorie yield per area.",
            70, 100, "Low", "Medium", "Medium", "Low",
            "Cool weather essential for tuber formation. Good long-term storage.",
            "Hill soil around stems. Harvest after vines die back.",
            "Seed potatoes, 10cm deep"),
        new("Broccoli", "Vegetables",
            "Cool-weather brassica. Frost tolerant and nutritious.",
            60, 90, "Low", "High", "Medium", "Low",
            "Heads best in cool weather. Side shoots after main harvest.",
            "Harvest central head before flowers open. Leave side shoots.",
            "Seedlings preferred"),
        new("Cabbage", "Vegetables",
            "Hardy cool-weather brassica. Long storage life.",
            70, 100, "Low", "High", "Medium", "Low",
            "Very cold-hardy. Stores for months in root cellar.",
            "Water consistently. Harvest when heads are firm.",
            "Seedlings or direct sow"),
        new("Peas", "Legumes",
            "Cool-weather legume. Sweetest when grown in cold.",
            55, 70, "Low", "High", "Medium", "Medium",
            "Love cool damp springs. Fix nitrogen for next crops.",
            "Provide trellis. Harvest regularly for continuous production.",
            "Direct sow early spring"),
        new("Beets", "Root Vegetables",
            "Dual-purpose root and greens. Handles cool weather well.",
            50, 70, "Low", "High", "Medium", "Medium",
            "Both roots and greens edible. Grows in cool compact soil.",
            "Soak seeds before planting. Harvest at golf-ball size.",
            "Direct sow, 2cm deep"),
        new("Onions", "Root Vegetables",
            "Cool-season bulb crop. Cured bulbs store up to 12 months.",
            90, 110, "Low", "Medium", "Medium", "Low",
            "Sets bulbs in response to day length. Choose right variety.",
            "Stop watering when tops fall over. Cure before storage.",
            "Sets or seedlings"),
        new("Lettuce", "Leafy Greens",
            "Quick cool-season leafy green. Many varieties for continuous harvest.",
            30, 50, "Low", "Medium", "Medium", "Medium",
            "Fastest salad crop. Bolts in heat — ideal for cool periods.",
            "Sow every 2 weeks. Harvest outer leaves for cut-and-come-again.",
            "Direct sow or seedlings"),

        // ===== FLOOD / WET TOLERANT =====
        new("Rice", "Grains",
            "Staple grain adapted to flooded conditions. High calorie yield.",
            120, 150, "Medium", "Low", "Medium", "High",
            "The classic flood-tolerant staple. Thrives in standing water.",
            "Keep soil saturated. Requires consistent water throughout.",
            "Seeds in flooded bed"),
        new("Taro", "Root Vegetables",
            "Starchy root crop for wet conditions. Bog plant.",
            200, 270, "Medium", "Low", "Medium", "High",
            "Thrives in waterlogged soil where other crops drown.",
            "Plant corms in wet soil or shallow standing water.",
            "Corms in wet soil"),
        new("Watercress", "Leafy Greens",
            "Aquatic leafy green rich in nutrients. Grows in flowing water.",
            30, 45, "Low", "Medium", "Medium", "High",
            "Grows in water — ideal for flood-prone areas or wet conditions.",
            "Grow in shallow running water or keep consistently wet.",
            "Cuttings in water"),
        new("Cranberry", "Fruits",
            "Perennial bog fruit. Requires acidic wet conditions.",
            365, 730, "Low", "High", "Low", "High",
            "Perennial that thrives in wet acidic conditions. Long-term investment.",
            "Needs acidic soil (pH 4-5). Keep consistently wet.",
            "Cuttings in prepared bog"),

        // ===== GENERAL / VERSATILE =====
        new("Tomatoes", "Vegetables",
            "Versatile warm-season fruit. Many varieties for different climates.",
            60, 85, "High", "Low", "Medium", "Low",
            "Warm-season staple. Dwarf varieties mature faster.",
            "Stake or cage. Remove suckers for larger fruits.",
            "Seedlings after frost"),
        new("Cucumber", "Vegetables",
            "Fast-growing warm-season vine. High yield per area.",
            50, 70, "Medium", "Low", "Medium", "Low",
            "Quick producer in warm weather. Trellising saves space.",
            "Harvest frequently for more production. Bitter = stress.",
            "Direct sow warm soil"),
        new("Peppers", "Vegetables",
            "Heat-loving fruit. Both sweet and hot varieties.",
            65, 90, "High", "Low", "Medium", "Low",
            "Hot peppers thrive in heat. Sweet peppers need consistent water.",
            "Harvest green or wait for color. Hotter = more sun.",
            "Seedlings after frost"),
        new("Zucchini / Summer Squash", "Vegetables",
            "Fast-growing prolific producer. Good for warm weather.",
            40, 55, "Medium", "Low", "Medium", "Low",
            "Extremely productive. Harvest daily once fruiting begins.",
            "Pick at 15-20cm. Check daily — they grow fast.",
            "Direct sow after frost"),
        new("Beans (Bush)", "Legumes",
            "Quick warm-season legume. Bush types mature fast.",
            45, 60, "Medium", "Low", "Medium", "Medium",
            "Quick protein source. Bush types need no staking.",
            "Sow weekly for continuous harvest. Pick pods regularly.",
            "Direct sow, 3-4cm deep"),
        new("Pumpkin / Winter Squash", "Vegetables",
            "Long-storage winter crop. Warm-season vine.",
            80, 120, "Medium", "Low", "Medium", "Low",
            "Excellent storage crop — keeps 3-6 months in cool place.",
            "Harvest after vines die back. Cure in sun for storage.",
            "Direct sow after frost"),

        // ===== HERBS (quick, resilient) =====
        new("Basil", "Herbs",
            "Fast-growing heat-loving herb. Repels pests naturally.",
            50, 75, "High", "Low", "Medium", "Low",
            "Quick aromatic herb. Pinch flowers for bushier growth.",
            "Pinch flower buds. Harvest from top down.",
            "Seedlings or direct sow"),
        new("Mint", "Herbs",
            "Extremely hardy perennial herb. Spreads aggressively.",
            60, 90, "Medium", "High", "Medium", "Medium",
            "Nearly impossible to kill. Tolerates heat and cold.",
            "Plant in container to control spread. Cut back regularly.",
            "Cuttings or divisions"),
        new("Rosemary", "Herbs",
            "Drought-tolerant woody perennial herb. Loves heat.",
            90, 180, "High", "Low", "High", "Low",
            "Mediterranean herb that thrives in dry heat. Perennial.",
            "Well-drained soil essential. Prune after flowering.",
            "Cuttings preferred"),
        new("Chard (Swiss Chard)", "Leafy Greens",
            "Heat and cold tolerant leafy green. Productive for months.",
            45, 60, "Medium", "Medium", "Medium", "Medium",
            "More heat-tolerant than spinach. Produces through summer.",
            "Harvest outer leaves. Cut 5cm above ground for regrowth.",
            "Direct sow or seedlings"),
    };

    // ── Agricultural regions for regional relevance boost ──
    private enum AgroRegion { Tropical, Arid, Mediterranean, Subtropical, Temperate, Boreal }

    private static readonly Dictionary<AgroRegion, HashSet<string>> RegionCrops = new()
    {
        [AgroRegion.Tropical] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Sweet Potato", "Cassava", "Okra", "Amaranth (Grain & Leaves)", "Sorghum", "Millet",
            "Cowpeas", "Pigeon Pea", "Taro", "Rice", "Eggplant",
            "Zucchini / Summer Squash", "Pumpkin / Winter Squash", "Beans (Bush)",
        },
        [AgroRegion.Arid] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Sorghum", "Millet", "Cowpeas", "Chickpeas", "Lentils", "Pigeon Pea",
            "Sesame", "Amaranth (Grain & Leaves)", "Sweet Potato",
            "Onions", "Rosemary",
        },
        [AgroRegion.Mediterranean] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Chickpeas", "Lentils", "Tomatoes", "Eggplant", "Peppers", "Zucchini / Summer Squash",
            "Onions", "Rosemary", "Basil", "Mint", "Lettuce", "Carrots",
            "Potatoes", "Kale", "Beets", "Chard (Swiss Chard)", "Pumpkin / Winter Squash",
        },
        [AgroRegion.Subtropical] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Rice", "Sweet Potato", "Cowpeas", "Okra", "Eggplant", "Peppers",
            "Taro", "Tomatoes", "Cucumber", "Beans (Bush)", "Pumpkin / Winter Squash",
            "Mint", "Basil", "Chard (Swiss Chard)", "Cabbage", "Carrots",
        },
        [AgroRegion.Temperate] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Potatoes", "Carrots", "Cabbage", "Broccoli", "Kale", "Spinach", "Lettuce",
            "Peas", "Beans (Bush)", "Beets", "Onions", "Tomatoes", "Cucumber",
            "Zucchini / Summer Squash", "Pumpkin / Winter Squash", "Peppers",
            "Basil", "Mint", "Chard (Swiss Chard)",
        },
        [AgroRegion.Boreal] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Potatoes", "Carrots", "Cabbage", "Kale", "Spinach", "Peas", "Beets",
            "Onions", "Lettuce", "Broccoli",
        },
    };

    // ── Country → region overrides for precise targeting ──
    private static readonly Dictionary<string, AgroRegion> CountryRegionOverride = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LK"] = AgroRegion.Tropical,       ["IN"] = AgroRegion.Subtropical,
        ["ID"] = AgroRegion.Tropical,       ["TH"] = AgroRegion.Tropical,
        ["VN"] = AgroRegion.Tropical,       ["PH"] = AgroRegion.Tropical,
        ["MY"] = AgroRegion.Tropical,       ["BR"] = AgroRegion.Tropical,
        ["NG"] = AgroRegion.Tropical,       ["GH"] = AgroRegion.Tropical,
        ["KE"] = AgroRegion.Tropical,       ["CO"] = AgroRegion.Tropical,
        ["EG"] = AgroRegion.Arid,           ["SA"] = AgroRegion.Arid,
        ["AE"] = AgroRegion.Arid,           ["SD"] = AgroRegion.Arid,
        ["ES"] = AgroRegion.Mediterranean,  ["PT"] = AgroRegion.Mediterranean,
        ["IT"] = AgroRegion.Mediterranean,  ["GR"] = AgroRegion.Mediterranean,
        ["TR"] = AgroRegion.Mediterranean,  ["MA"] = AgroRegion.Mediterranean,
        ["CN"] = AgroRegion.Subtropical,    ["JP"] = AgroRegion.Subtropical,
        ["KR"] = AgroRegion.Subtropical,    ["AU"] = AgroRegion.Subtropical,
        ["ZA"] = AgroRegion.Subtropical,    ["AR"] = AgroRegion.Subtropical,
        ["CL"] = AgroRegion.Mediterranean,
        ["US"] = AgroRegion.Temperate,      ["CA"] = AgroRegion.Temperate,
        ["GB"] = AgroRegion.Temperate,      ["DE"] = AgroRegion.Temperate,
        ["FR"] = AgroRegion.Temperate,      ["NL"] = AgroRegion.Temperate,
        ["SE"] = AgroRegion.Boreal,         ["NO"] = AgroRegion.Boreal,
        ["FI"] = AgroRegion.Boreal,         ["RU"] = AgroRegion.Boreal,
    };

    private static AgroRegion DetermineRegion(double lat, double lng, string? countryCode)
    {
        if (countryCode != null && CountryRegionOverride.TryGetValue(countryCode, out var region))
            return region;

        // Fall back to latitude-based zone detection
        var absLat = Math.Abs(lat);
        if (absLat < 23.5) return AgroRegion.Tropical;
        if (absLat < 30)   return AgroRegion.Subtropical;
        if (absLat < 35 && lng is > -10 and < 40) return AgroRegion.Mediterranean;
        if (absLat < 50)   return AgroRegion.Temperate;
        return AgroRegion.Boreal;
    }

    public async Task<CropRecommendationResponse> GetCropRecommendationsAsync(ClimateForecast forecast, string? countryCode = null, CancellationToken ct = default)
    {
        var signals = forecast.DetectedSignals;
        var tempAnomaly = forecast.TemperatureAnomaly ?? 0;
        var precipAnomaly = forecast.PrecipitationAnomaly ?? 0;
        var isElNino = signals.Contains(ClimateSignal.ElNino);
        var isLaNina = signals.Contains(ClimateSignal.LaNina);
        var isDrought = signals.Contains(ClimateSignal.Drought) || signals.Contains(ClimateSignal.ExtremeDrought);
        var isFloodRisk = signals.Contains(ClimateSignal.FloodRisk);
        var isStorm = signals.Contains(ClimateSignal.StormRisk);
        var isHeatwave = signals.Contains(ClimateSignal.Heatwave);
        var isCold = signals.Contains(ClimateSignal.ColdSpell);
        var isHeavyRain = signals.Contains(ClimateSignal.HeavyRainfall);
        var isNormal = signals.Contains(ClimateSignal.Normal) || signals.Count == 0;

        var now = DateTime.UtcNow;
        var plantByDate = now.Date;

        // Determine agricultural region once for all scoring & Gemini
        var region = DetermineRegion(forecast.Latitude, forecast.Longitude, countryCode);
        var regionName = region switch
        {
            AgroRegion.Tropical => "Tropical",
            AgroRegion.Arid => "Arid",
            AgroRegion.Mediterranean => "Mediterranean",
            AgroRegion.Subtropical => "Subtropical",
            AgroRegion.Temperate => "Temperate",
            AgroRegion.Boreal => "Boreal/Cold",
            _ => "Unknown"
        };

        var scored = new List<(CropEntry Crop, int Score, string Reason, string Tip)>();

        foreach (var crop in CropCatalog)
        {
            int score = 50; // baseline
            var reasons = new List<string>();

            // ── Regional relevance boost ──
            if (RegionCrops.TryGetValue(region, out var regionCrops) && regionCrops.Contains(crop.Name))
            {
                score += 15;
                reasons.Add($"Commonly grown in your region — proven local success");
            }

            // ── Heat scoring ──
            if (isHeatwave || (isElNino && tempAnomaly > 0.5))
            {
                if (crop.HeatTolerance == "High")
                {
                    score += 25;
                    reasons.Add("Thrives in hot conditions");
                }
                else if (crop.HeatTolerance == "Low")
                {
                    score -= 30;
                    reasons.Add("Will struggle in high heat");
                }
            }

            // ── Cold scoring ──
            if (isCold || (isLaNina && tempAnomaly < -0.5))
            {
                if (crop.ColdTolerance == "High")
                {
                    score += 25;
                    reasons.Add("Handles cold well");
                }
                else if (crop.ColdTolerance == "Low")
                {
                    score -= 25;
                    reasons.Add("Damaged by frost/cold");
                }
            }

            // ── Drought scoring ──
            if (isDrought || (isElNino && precipAnomaly < -10))
            {
                if (crop.DroughtTolerance == "High")
                {
                    score += 30;
                    reasons.Add("Excellent drought tolerance");
                }
                else if (crop.DroughtTolerance == "Low")
                {
                    score -= 25;
                    reasons.Add("Requires regular watering");
                }
            }

            // ── Flood/wet scoring ──
            if (isFloodRisk || isHeavyRain || (isLaNina && precipAnomaly > 10))
            {
                if (crop.FloodTolerance == "High")
                {
                    score += 30;
                    reasons.Add("Thrives in wet/flooded conditions");
                }
                else if (crop.FloodTolerance == "Low")
                {
                    score -= 20;
                    reasons.Add("Will rot or drown in wet conditions");
                }
            }

            // ── Storm scoring ──
            if (isStorm)
            {
                if (crop is { Height: var h, IsVine: var vine } && (h < 50 || vine))
                {
                    score += 10;
                    reasons.Add("Low-growing or vining — less storm damage");
                }
                else
                {
                    score -= 10;
                    reasons.Add("Tall plants may suffer wind damage");
                }
            }

            // ── Normal conditions ──
            if (isNormal)
            {
                score += 10; // baseline favorable
                reasons.Add("Normal conditions — ideal for most crops");
            }

            // Clamp score 0-100
            score = Math.Clamp(score, 0, 100);

            // Build recommendation reason
            var reason = reasons.Count > 0
                ? string.Join(". ", reasons.Distinct()) + "."
                : "Suitable for general conditions.";

            // Build growing tip
            var tip = crop.GrowingTip;

            if (isDrought && crop.DroughtTolerance == "High")
                tip = "Use deep mulch and water deeply once weekly. " + crop.PlantingMethod;
            else if (isFloodRisk && crop.FloodTolerance == "High")
                tip = "Plant in raised beds or mounds to control water flow. " + crop.PlantingMethod;
            else if (isHeatwave && crop.HeatTolerance == "High")
                tip = "Provide afternoon shade in extreme heat. " + crop.PlantingMethod;
            else if (isCold && crop.ColdTolerance == "High")
                tip = "Use row covers or cloches for frost protection. " + crop.PlantingMethod;
            else
                tip = crop.PlantingMethod + ". " + crop.GrowingTip;

            if (score >= 30) // only recommend decent matches
                scored.Add((crop, score, reason, tip));
        }

        // Sort by score descending, then by days to harvest ascending
        scored.Sort((a, b) =>
        {
            var cmp = b.Score.CompareTo(a.Score);
            return cmp != 0 ? cmp : a.Crop.DaysMin.CompareTo(b.Crop.DaysMin);
        });

        // ── Gemini LLM reranking (enhances rule engine results) ──
        if (_gemini.IsEnabled)
        {
            // Take top 20 for Gemini to rerank (keep others as-is)
            var topCrops = scored.Take(20).Select(s => (s.Crop.Name, s.Score, s.Reason, s.Tip)).ToList();
            var geminiResult = await _gemini.RerankCropsAsync(forecast, countryCode, regionName, topCrops, ct);

            if (geminiResult?.RerankedCrops != null && geminiResult.RerankedCrops.Count > 0)
            {
                _logger.LogInformation("Gemini reranked {Count} crops — applying LLM enhancements", geminiResult.RerankedCrops.Count);

                // Build a lookup of Gemini results by crop name
                var geminiLookup = geminiResult.RerankedCrops
                    .Where(g => g.SuitabilityScore >= 0)
                    .ToDictionary(g => g.CropName, StringComparer.OrdinalIgnoreCase);

                // Rebuild scored list: keep LLM scores for crops Gemini saw, rule scores for the rest
                var geminiScored = new List<(CropEntry Crop, int Score, string Reason, string Tip)>();
                foreach (var s in scored)
                {
                    if (geminiLookup.TryGetValue(s.Crop.Name, out var gem))
                    {
                        var tip = !string.IsNullOrEmpty(gem.GrowingTip)
                            ? gem.GrowingTip
                            : s.Tip;
                        var reason = !string.IsNullOrEmpty(gem.RecommendationReason)
                            ? gem.RecommendationReason
                            : s.Reason;
                        geminiScored.Add((s.Crop, Math.Clamp(gem.SuitabilityScore, 0, 100), reason, tip));
                    }
                    else
                    {
                        geminiScored.Add(s);
                    }
                }

                // Re-sort by the updated (Gemini) scores
                geminiScored.Sort((a, b) =>
                {
                    var cmp = b.Score.CompareTo(a.Score);
                    return cmp != 0 ? cmp : a.Crop.DaysMin.CompareTo(b.Crop.DaysMin);
                });

                scored = geminiScored;
            }
        }

        var recommendations = new List<CropRecommendation>();

        // Fetch all images with rate-limited concurrency
        const int maxConcurrent = 5;
        using var imageSemaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        var imageTasks = scored.Select(s => FetchCropImageWithThrottleAsync(s.Crop.Name, imageSemaphore, ct));
        var imageResults = await Task.WhenAll(imageTasks);
        var imageMap = new Dictionary<string, (string? ImageUrl, string? WikiUrl)>();
        foreach (var (name, result) in imageResults)
            imageMap[name] = result;

        // Build recommendations
        foreach (var s in scored)
        {
            var harvestStart = plantByDate.AddDays(s.Crop.DaysMin);
            var harvestEnd = plantByDate.AddDays(s.Crop.DaysMax);
            var (imageUrl, wikiUrl) = imageMap.TryGetValue(s.Crop.Name, out var imgResult) ? imgResult : (null, null);

            recommendations.Add(new CropRecommendation
            {
                CropName = s.Crop.Name,
                Category = s.Crop.Category,
                Description = s.Crop.Description,
                DaysToHarvestMin = s.Crop.DaysMin,
                DaysToHarvestMax = s.Crop.DaysMax,
                PlantByDate = plantByDate,
                HarvestStartDate = harvestStart,
                HarvestEndDate = harvestEnd,
                HeatTolerance = s.Crop.HeatTolerance,
                ColdTolerance = s.Crop.ColdTolerance,
                DroughtTolerance = s.Crop.DroughtTolerance,
                FloodTolerance = s.Crop.FloodTolerance,
                SuitabilityScore = s.Score,
                RecommendationReason = s.Reason,
                GrowingTip = s.Tip,
                PlantingMethod = s.Crop.PlantingMethod,
                ImageUrl = imageUrl,
                WikiUrl = wikiUrl,
            });
        }

        // General advice
        var generalAdvice = BuildGeneralAdvice(signals, tempAnomaly, precipAnomaly);

        return new CropRecommendationResponse
        {
            Crops = recommendations,
            TotalCrops = recommendations.Count,
            GeneralAdvice = generalAdvice,
        };
    }

    /// <summary>Fetch a crop image from Wikipedia API with rate-limited throttling.</summary>
    private async Task<(string Name, (string? ImageUrl, string? WikiUrl) Result)> FetchCropImageWithThrottleAsync(
        string cropName, SemaphoreSlim semaphore, CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            var result = await GetCropImageAsync(cropName, ct);
            return (cropName, result);
        }
        finally
        {
            // Small delay between releases to stagger the next batch
            await Task.Delay(150, ct);
            semaphore.Release();
        }
    }

    /// <summary>Fetch a crop image from Wikipedia API, with in-memory caching of successful results only.</summary>
    private async Task<(string? ImageUrl, string? WikiUrl)> GetCropImageAsync(string cropName, CancellationToken ct)
    {
        // Check cache (only hits if we previously got a valid image)
        if (ImageCache.TryGetValue(cropName, out var cached))
            return cached;

        try
        {
            var wikiTitle = MapToWikiTitle(cropName);
            var url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{wikiTitle}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.UserAgent.ParseAdd("ClimateSurvival/1.0 (crop-image-fetcher)");

            using var response = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return (null, null); // Don't cache failures — allow retry next time

            var json = await response.Content.ReadFromJsonAsync<WikipediaSummary>(cancellationToken: ct);
            var imageUrl = json?.Thumbnail?.Source;
            var wikiUrl = json?.ContentUrls?.Desktop?.Page;

            // Only cache if we got a valid image URL
            if (!string.IsNullOrEmpty(imageUrl))
                ImageCache.TryAdd(cropName, (imageUrl, wikiUrl));

            return (imageUrl, wikiUrl);
        }
        catch
        {
            return (null, null); // Don't cache failures — allow retry next time
        }
    }

    private static string MapToWikiTitle(string cropName) => cropName switch
    {
        "Sweet Potato" => "Sweet_potato",
        "Okra" => "Okra",
        "Amaranth (Grain & Leaves)" => "Amaranth",
        "Sorghum" => "Sorghum",
        "Millet" => "Millet",
        "Cassava" => "Cassava",
        "Cowpeas" => "Cowpea",
        "Chickpeas" => "Chickpea",
        "Lentils" => "Lentil",
        "Pigeon Pea" => "Pigeon_pea",
        "Sesame" => "Sesame",
        "Quinoa" => "Quinoa",
        "Eggplant" => "Eggplant",
        "Kale" => "Kale",
        "Spinach" => "Spinach",
        "Carrots" => "Carrot",
        "Potatoes" => "Potato",
        "Broccoli" => "Broccoli",
        "Cabbage" => "Cabbage",
        "Peas" => "Pea",
        "Beets" => "Beetroot",
        "Onions" => "Onion",
        "Lettuce" => "Lettuce",
        "Rice" => "Rice",
        "Taro" => "Taro",
        "Watercress" => "Watercress",
        "Cranberry" => "Cranberry",
        "Tomatoes" => "Tomato",
        "Cucumber" => "Cucumber",
        "Peppers" => "Bell_pepper",
        "Zucchini / Summer Squash" => "Zucchini",
        "Beans (Bush)" => "Bean",
        "Pumpkin / Winter Squash" => "Pumpkin",
        "Basil" => "Basil",
        "Mint" => "Mentha", // Wikipedia uses "Mentha" for the genus
        "Rosemary" => "Rosemary",
        "Chard (Swiss Chard)" => "Chard",
        _ => cropName.Replace(" ", "_")
    };

    private static string BuildGeneralAdvice(List<ClimateSignal> signals, double? tempAnomaly, double? precipAnomaly)
    {
        if (signals.Contains(ClimateSignal.ExtremeDrought))
            return "EXTREME DROUGHT: Prioritize deep-rooted, drought-tolerant crops. Install drip irrigation immediately. "
                 + "Focus on crops with <90 days to harvest. Consider storing water for irrigation.";
        if (signals.Contains(ClimateSignal.Drought))
            return "DROUGHT CONDITIONS: Grow drought-resistant grains and root crops. Use heavy mulch and water-conserving "
                 + "techniques. Quick-maturing varieties (30-60 days) are recommended.";
        if (signals.Contains(ClimateSignal.FloodRisk))
            return "FLOOD RISK: Prioritize flood-tolerant crops (rice, taro, watercress). Plant in raised beds or on mounds. "
                 + "Avoid root vegetables in low-lying areas — they will rot.";
        if (signals.Contains(ClimateSignal.StormRisk))
            return "STORM WARNING: Grow low-profile and vining crops that resist wind damage. Stake all tall plants securely. "
                 + "Build windbreaks if possible.";
        if (signals.Contains(ClimateSignal.Heatwave) || (tempAnomaly ?? 0) > 2)
            return "EXTREME HEAT: Focus on heat-loving crops (okra, sweet potato, amaranth, sorghum). Provide shade for "
                 + "sensitive plants. Water early morning or evening.";
        if (signals.Contains(ClimateSignal.ElNino))
            return "EL NIÑO PATTERN: Expect warmer, drier conditions. Plant heat and drought tolerant varieties. "
                 + "Consider water storage. Fast-maturing crops reduce risk.";
        if (signals.Contains(ClimateSignal.LaNina))
            return "LA NIÑA PATTERN: Expect cooler, wetter conditions. Prioritize flood-tolerant and cool-weather crops. "
                 + "Good time for leafy greens, root veg, and brassicas.";
        if (signals.Contains(ClimateSignal.ColdSpell))
            return "COLD SPELL: Focus on cold-hardy crops (kale, spinach, carrots, peas, cabbage). Use row covers "
                 + "and cold frames. Avoid warm-season crops until temperatures rise.";
        if (signals.Contains(ClimateSignal.Normal))
            return "NORMAL CONDITIONS: All crops are viable. This is a good time to establish a diverse food garden. "
                 + "Balance quick crops (lettuce, spinach, beans) with long-term storage crops (potatoes, sweet potatoes, winter squash).";

        return "GENERAL CONDITIONS: A balanced selection of crops is recommended. Include both quick-growing "
             + "vegetables and long-storage staples for food security.";
    }
}

/// <summary>Wikipedia REST API /page/summary response model</summary>
public class WikipediaSummary
{
    public WikipediaThumbnail? Thumbnail { get; set; }
    public WikipediaContentUrls? ContentUrls { get; set; }
}

public class WikipediaThumbnail
{
    public string? Source { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class WikipediaContentUrls
{
    public WikipediaPageUrl? Desktop { get; set; }
}

public class WikipediaPageUrl
{
    public string? Page { get; set; }
    public string? Revision { get; set; }
    public string? Edit { get; set; }
    public string? Talk { get; set; }
}

/// <summary>Internal crop catalog entry</summary>
public class CropEntry
{
    public string Name { get; }
    public string Category { get; }
    public string Description { get; }
    public int DaysMin { get; }
    public int DaysMax { get; }
    public string HeatTolerance { get; }
    public string ColdTolerance { get; }
    public string DroughtTolerance { get; }
    public string FloodTolerance { get; }
    public string GrowingTip { get; }
    public string PlantingMethod { get; }
    public int Height { get; set; } = 60; // cm, approximate
    public bool IsVine { get; set; }

    public CropEntry(string name, string category, string description,
        int daysMin, int daysMax,
        string heatTol, string coldTol, string droughtTol, string floodTol,
        string growingTip, string plantingMethod, string? plantingNote = null)
    {
        Name = name;
        Category = category;
        Description = description;
        DaysMin = daysMin;
        DaysMax = daysMax;
        HeatTolerance = heatTol;
        ColdTolerance = coldTol;
        DroughtTolerance = droughtTol;
        FloodTolerance = floodTol;
        GrowingTip = growingTip;
        PlantingMethod = plantingMethod;

        // Infer height/vine from category/name
        if (name.Contains("cucumber") || name.Contains("pumpkin") || name.Contains("squash") || name.Contains("pea"))
            IsVine = true;
        else if (category is "Grains" or "Legumes")
            Height = 80 + (name == "Sorghum" ? 120 : name == "Millet" ? 100 : name.StartsWith("Pigeon") ? 200 : 60);
        else if (category is "Root Vegetables")
            Height = 30;
        else if (category is "Leafy Greens")
            Height = 25;
        else if (category is "Herbs")
            Height = name == "Rosemary" ? 120 : 40;
        else if (name is "Okra" or "Eggplant" or "Peppers")
            Height = 90 + (name == "Okra" ? 120 : 0);
        else if (name == "Tomatoes")
        {
            Height = 120;
            IsVine = true;
        }
    }
}

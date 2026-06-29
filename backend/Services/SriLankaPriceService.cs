using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace ClimateAdvisor.Api.Services;

public class PriceEntry
{
    public string Market { get; set; } = "";
    public decimal? Yesterday { get; set; }
    public decimal? Today { get; set; }
    public decimal? Change => Today.HasValue && Yesterday.HasValue
        ? Math.Round(Today.Value - Yesterday.Value, 2)
        : null;
}

public class MarketPrice
{
    public string Commodity { get; set; } = "";
    public string Category { get; set; } = "Other";
    public string Unit { get; set; } = "Rs./kg";
    public List<PriceEntry> Prices { get; set; } = new();
}

public class SriLankaPriceReport
{
    public string Source { get; set; } = "Central Bank of Sri Lanka - Daily Price Report";
    public DateTime ReportDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public int TotalItems { get; set; }
    public List<MarketPrice> Commodities { get; set; } = new();
}

public class SriLankaPriceService
{
    private readonly HttpClient _http;
    private readonly ILogger<SriLankaPriceService> _log;
    private SriLankaPriceReport? _cached;
    private DateTime _lastFetch = DateTime.MinValue;

    private const string ReportBaseUrl =
        "https://www.cbsl.gov.lk/sites/default/files/cbslweb_documents/statistics/pricerpt/price_report_";

    private static readonly string[] Markets = { "Dambulla", "Nuwara Eliya", "Kandy", "Pettah", "Jaffna" };

    // Regex: word (letters/spaces/parens/dots) then Rs.unit then first number
    private static readonly Regex CommodityRegex = new(
        @"([A-Za-z][A-Za-z\s\(\)\.\*]+?)(Rs\.[/A-Za-z]+)([\d,]+\.?\d*)",
        RegexOptions.Compiled);

    // Regex to find individual numbers in remaining text
    private static readonly Regex NumberRegex = new(
        @"[\d,]+\.?\d*",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, string> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Beans"] = "Vegetables",
        ["Carrot"] = "Vegetables",
        ["Cabbage"] = "Vegetables",
        ["Tomato"] = "Vegetables",
        ["Brinjal"] = "Vegetables",
        ["Pumpkin"] = "Vegetables",
        ["Snake Gourd"] = "Vegetables",
        ["Green Chilli"] = "Vegetables",
        ["Lime"] = "Fruits",
        ["Red Onion (Local)"] = "Vegetables",
        ["Red Onion (Imp)"] = "Vegetables",
        ["Big Onion (Local)"] = "Vegetables",
        ["Big Onion (Imp)"] = "Vegetables",
        ["Potato (Local)"] = "Vegetables",
        ["Potato (Imp)"] = "Vegetables",
        ["Dried Chilli (Imp)"] = "Spices",
        ["Coconut (Avg.)"] = "Other",
        ["Coconut Oil"] = "Oils & Fats",
        ["Red Dhal"] = "Grains",
        ["Sugar (White)"] = "Other",
        ["Egg (White)"] = "Protein",
        ["Eggs (Hen)"] = "Protein",
        ["Sprat (Imp)"] = "Fish",
        ["Katta (Imp)"] = "Fish",
        ["Banana (Sour)"] = "Fruits",
        ["Papaw"] = "Fruits",
        ["Pineapple"] = "Fruits",
        ["Apple (Imp)"] = "Fruits",
        ["Orange (Imp)"] = "Fruits",
        ["Samba"] = "Rice",
        ["Nadu"] = "Rice",
        ["Kekulu (White)"] = "Rice",
        ["Kekulu (Red)"] = "Rice",
        ["Ponni Samba (Imp)"] = "Rice",
        ["Nadu (Imp)"] = "Rice",
        ["Kekulu (White) (Imp)"] = "Rice",
        ["Kelawalla"] = "Fish",
        ["Thalapath"] = "Fish",
        ["Balaya"] = "Fish",
        ["Paraw"] = "Fish",
        ["Salaya"] = "Fish",
        ["Hurulla"] = "Fish",
        ["Linna"] = "Fish",
        ["Big Onion"] = "Vegetables",
        ["Dried Chilli"] = "Spices",
        ["Coconut"] = "Other",
        ["Egg"] = "Protein",
        ["Dhal"] = "Grains",
        ["Sugar"] = "Other",
        ["Potato"] = "Vegetables",
    };

    public SriLankaPriceService(HttpClient http, ILogger<SriLankaPriceService> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<SriLankaPriceReport?> GetLatestPricesAsync(CancellationToken ct = default)
    {
        if (_cached != null && DateTime.UtcNow - _lastFetch < TimeSpan.FromHours(6))
            return _cached;

        var today = DateTime.UtcNow.Date;
        if (DateTime.UtcNow.TimeOfDay < TimeSpan.FromHours(5.5))
            today = today.AddDays(-1);

        try
        {
            for (int ago = 0; ago < 10; ago++)
            {
                var date = today.AddDays(-ago);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var url = $"{ReportBaseUrl}{date:yyyyMMdd}_e.pdf";
                _log.LogInformation("Attempting CBSL report: {Url}", url);

                using var resp = await _http.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) continue;

                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                var report = ParseReport(stream, date);
                if (report?.Commodities.Count > 0)
                {
                    _cached = report;
                    _lastFetch = DateTime.UtcNow;
                    _log.LogInformation("Loaded CBSL report from {Date} with {Count} items",
                        date.ToString("yyyy-MM-dd"), report.Commodities.Count);
                    return report;
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to fetch CBSL price report");
        }

        return _cached;
    }

    internal static SriLankaPriceReport? ParseReport(Stream pdfStream, DateTime reportDate)
    {
        using var pdf = PdfDocument.Open(pdfStream);
        for (int pg = 2; pg <= Math.Min(pdf.NumberOfPages, 3); pg++)
        {
            var page = pdf.GetPage(pg);
            var text = page.Text;
            if (string.IsNullOrWhiteSpace(text)) continue;

            var commodities = ParseCommodities(text);
            if (commodities.Count > 0)
            {
                return new SriLankaPriceReport
                {
                    ReportDate = reportDate,
                    LastUpdated = DateTime.UtcNow,
                    TotalItems = commodities.Count,
                    Commodities = commodities,
                };
            }
        }
        return null;
    }

    internal static List<MarketPrice> ParseCommodities(string pageText)
    {
        var result = new List<MarketPrice>();
        int pos = 0;

        while (true)
        {
            var match = CommodityRegex.Match(pageText, pos);
            if (!match.Success) break;

            var rawName = match.Groups[1].Value.Trim();
            var unit = match.Groups[2].Value;
            var firstValStr = match.Groups[3].Value;

            // Skip the first fake match which is the header line merged with "Beans"
            if (rawName.Contains("Yesterday") || rawName.Contains("Wholesale"))
            {
                pos = match.Index + match.Length;
                continue;
            }

            // Clean the commodity name: strip leading "n.a." patterns
            var name = StripNA(rawName);
            if (string.IsNullOrWhiteSpace(name) || !IsKnownCommodity(name))
            {
                pos = match.Index + match.Length;
                continue;
            }

            // Fix unit which might have trailing characters from next "n.a."
            unit = FixUnit(unit);

            // Extract all price values from the remaining text after the unit
            var afterFirstVal = match.Index + match.Length;

            // Collect numbers: start with the first value from the regex
            var values = new List<decimal?>();
            if (TryParsePrice(firstValStr, out var fv))
                values.Add(fv);

            // Then scan for more numbers in the remaining text
            var scanPos = afterFirstVal;
            while (scanPos < pageText.Length)
            {
                // Skip whitespace
                while (scanPos < pageText.Length && char.IsWhiteSpace(pageText[scanPos]))
                    scanPos++;
                if (scanPos >= pageText.Length) break;

                // Check for n.a.
                if (scanPos + 3 <= pageText.Length &&
                    pageText[scanPos..(scanPos + 3)].Equals("n.a", StringComparison.OrdinalIgnoreCase))
                {
                    values.Add(null);
                    scanPos += 3; // skip "n.a"
                    // also skip trailing period/dot
                    if (scanPos < pageText.Length && pageText[scanPos] == '.')
                        scanPos++;
                    continue;
                }

                // Try to match a number
                var numMatch = NumberRegex.Match(pageText, scanPos);
                if (!numMatch.Success || numMatch.Index != scanPos)
                    break; // Not a number → next commodity starts

                if (TryParsePrice(numMatch.Value, out var nv))
                    values.Add(nv);
                scanPos = numMatch.Index + numMatch.Length;
            }

            if (values.Count < 2)
            {
                pos = match.Index + match.Length;
                continue;
            }

            // Map prices to markets (2 values per market: yesterday + today)
            var marketPrices = new List<PriceEntry>();
            var marketCount = Math.Min(values.Count / 2, Markets.Length);

            for (int m = 0; m < marketCount; m++)
            {
                marketPrices.Add(new PriceEntry
                {
                    Market = Markets[m],
                    Yesterday = values[m * 2],
                    Today = values[m * 2 + 1],
                });
            }

            result.Add(new MarketPrice
            {
                Commodity = NormalizeName(name),
                Category = CategoryMap.TryGetValue(name, out var cat) ? cat
                    : CategoryMap.TryGetValue(NormalizeName(name), out var cat2) ? cat2
                    : "Other",
                Unit = unit,
                Prices = marketPrices,
            });

            pos = match.Index + match.Length;
        }

        return result;
    }

    private static string StripNA(string raw)
    {
        // Remove leading "n.a." patterns
        var result = raw.Trim();
        while (result.StartsWith("n.a.", StringComparison.OrdinalIgnoreCase))
            result = result[4..].Trim();
        while (result.StartsWith("n.a", StringComparison.OrdinalIgnoreCase))
            result = result[3..].Trim();
        return result.Trim();
    }

    private static string FixUnit(string unit)
    {
        // Handle cases where trailing character from "n.a." got appended
        // e.g., "Rs./kgn" → "Rs./kg"
        if (unit.EndsWith("kgn") || unit.EndsWith("kg."))
            return "Rs./kg";
        return unit;
    }

    private static bool TryParsePrice(string s, out decimal val)
    {
        val = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        var cleaned = s.Replace(",", "").Trim();
        return decimal.TryParse(cleaned,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out val);
    }

    private static readonly HashSet<string> KnownCommodities = new(StringComparer.OrdinalIgnoreCase)
    {
        "Beans", "Carrot", "Cabbage", "Tomato", "Brinjal", "Pumpkin",
        "Snake Gourd", "Green Chilli", "Lime",
        "Red Onion (Local)", "Red Onion (Imp)",
        "Big Onion (Local)", "Big Onion (Imp)", "Big Onion",
        "Potato (Local)", "Potato (Imp)", "Potato",
        "Dried Chilli (Imp)", "Dried Chilli",
        "Coconut (Avg.)", "Coconut", "Coconut Oil", "Coconut oil",
        "Red Dhal", "Dhal",
        "Sugar (White)", "Sugar",
        "Egg (White)", "Egg", "Eggs (Hen)",
        "Katta (Imp)", "Sprat (Imp)",
        "Banana (Sour)", "Papaw", "Pineapple",
        "Apple (Imp)", "Orange (Imp)",
        "Samba", "Nadu", "Kekulu (White)", "Kekulu (Red)",
        "Ponni Samba (Imp)", "Nadu (Imp)", "Kekulu (White) (Imp)",
        "Kelawalla", "Thalapath", "Balaya", "Paraw", "Salaya", "Hurulla", "Linna",
    };

    private static bool IsKnownCommodity(string name)
    {
        name = name.Trim();
        if (KnownCommodities.Contains(name)) return true;
        foreach (var known in KnownCommodities)
        {
            if (name.StartsWith(known, StringComparison.OrdinalIgnoreCase) ||
                known.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string NormalizeName(string name)
    {
        return name.Trim() switch
        {
            "Snake gourd" => "Snake Gourd",
            "Red Onion" => "Red Onion (Local)",
            "Red Onion (lmp)" => "Red Onion (Imp)",
            "Big Onion" => "Big Onion (Imp)",
            "Potato" => "Potato (Imp)",
            "Dried Chilli" => "Dried Chilli (Imp)",
            "Coconut" => "Coconut (Avg.)",
            "Coconut oil" => "Coconut Oil",
            "Egg" => "Egg (White)",
            "Eggs" => "Egg (White)",
            "Dhal" => "Red Dhal",
            "Sugar" => "Sugar (White)",
            _ => name.Trim(),
        };
    }
}

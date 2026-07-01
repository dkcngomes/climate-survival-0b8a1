namespace ClimateAdvisor.Api.Models;

/// <summary>Supported carriers with their email-to-SMS gateway domains.</summary>
public static class CarrierInfo
{
    /// <summary>Get the list of supported carriers.</summary>
    public static List<CarrierEntry> All => new()
    {
        // Sri Lanka
        new("Dialog Sri Lanka", "dialog", "dialog.lk"),
        new("Mobitel Sri Lanka", "mobitel", "sms.mobitel.lk"),
        new("Hutch Sri Lanka", "hutch", "hutch.lk"),
        new("Airtel Sri Lanka", "airtel", "airtel.lk"),

        // India
        new("Airtel India", "airtel-in", "airtel.in"),
        new("Jio India", "jio", "jio.com"),
        new("VI India", "vi", "vi.net"),
        new("BSNL India", "bsnl", "bsnl.in"),

        // US / Canada
        new("AT&T", "att", "txt.att.net"),
        new("Verizon", "verizon", "vtext.com"),
        new("T-Mobile", "tmobile", "tmomail.net"),
        new("Sprint", "sprint", "messaging.sprintpcs.com"),
        new("Bell Canada", "bell", "txt.bell.ca"),
        new("Rogers Canada", "rogers", "pcs.rogers.com"),

        // UK
        new("Vodafone UK", "vodafone-uk", "vodafone.co.uk"),
        new("EE UK", "ee", "ee.co.uk"),
        new("O2 UK", "o2", "o2.co.uk"),
        new("Three UK", "three", "three.co.uk"),

        // Australia / NZ
        new("Telstra Australia", "telstra", "telstra.com"),
        new("Optus Australia", "optus", "optus.com.au"),
        new("Vodafone Australia", "vodafone-au", "vodafone.com.au"),
        new("Spark NZ", "spark", "spark.co.nz"),
        new("Vodafone NZ", "vodafone-nz", "vodafone.co.nz"),

        // Other
        new("MTN South Africa", "mtn", "mtn.co.za"),
        new("Vodacom South Africa", "vodacom", "vodacom.co.za"),
        new("Globe Philippines", "globe", "globetxt.com"),
        new("Smart Philippines", "smart", "smart.com.ph"),
    };

    /// <summary>Resolve the gateway email address for a phone number and carrier code.</summary>
    public static string? ToSmsEmail(string phoneNumber, string carrierCode)
    {
        var carrier = All.FirstOrDefault(c =>
            c.Code.Equals(carrierCode, StringComparison.OrdinalIgnoreCase));
        if (carrier == null) return null;

        // Strip any non-digit characters from phone number
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digits)) return null;

        return $"{digits}@{carrier.GatewayDomain}";
    }
}

public record CarrierEntry(string Name, string Code, string GatewayDomain);

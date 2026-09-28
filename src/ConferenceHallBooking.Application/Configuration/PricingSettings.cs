namespace ConferenceHallBooking.Application.Configuration;

/// <summary>
/// Pricing configuration. Tariff rules are seeded from this section into the database;
/// the time zone identifies where tariff windows are evaluated.
/// </summary>
public class PricingSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "PricingSettings";

    /// <summary>IANA time zone id in which tariff windows are evaluated.</summary>
    public string TimeZoneId { get; set; } = "Europe/Kyiv";
}

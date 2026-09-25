namespace ConferenceHallBooking.Application.Configuration;

/// <summary>
/// JWT token configuration loaded from appsettings.json.
/// </summary>
public class JwtSettings
{
    /// <summary>Configuration section name in appsettings.json.</summary>
    public const string SectionName = "JwtSettings";

    /// <summary>Signing key used to issue and validate tokens.</summary>
    public string SecretKey { get; set; } = string.Empty;
    /// <summary>Value of the issuer claim.</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>Value of the audience claim.</summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>Token lifetime in minutes.</summary>
    public int ExpirationInMinutes { get; set; } = 60;
}

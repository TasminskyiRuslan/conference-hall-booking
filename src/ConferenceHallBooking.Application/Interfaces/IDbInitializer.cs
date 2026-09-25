namespace ConferenceHallBooking.Application.Interfaces;

/// <summary>
/// Applies pending migrations and seeds reference data on application startup.
/// </summary>
public interface IDbInitializer
{
    /// <summary>Applies pending migrations and seeds reference data when missing.</summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}

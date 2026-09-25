namespace ConferenceHallBooking.Application.Interfaces;

/// <summary>
/// Seeds the database with initial data on first run.
/// </summary>
public interface IDbInitializer
{
    /// <summary>Applies pending migrations and seeds reference data when missing.</summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}

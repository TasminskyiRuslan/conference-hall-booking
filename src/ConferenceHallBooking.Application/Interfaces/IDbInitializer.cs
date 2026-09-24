namespace ConferenceHallBooking.Application.Interfaces;

/// <summary>
/// Seeds the database with initial data on first run.
/// </summary>
public interface IDbInitializer
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

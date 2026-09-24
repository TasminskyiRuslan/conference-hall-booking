namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Coordinates transactions across multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

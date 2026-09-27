namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Persists pending changes produced across multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists pending changes; returns the number of affected rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

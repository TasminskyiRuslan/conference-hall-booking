namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Coordinates transactions across multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the given work inside a database transaction and commits on success.
    /// Rolls back on failure.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}

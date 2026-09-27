using ConferenceHallBooking.Domain.Interfaces;

namespace ConferenceHallBooking.Infrastructure.Data;

/// <summary>
/// EF Core implementation of the Unit of Work pattern. Commits all pending changes to the database.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <summary>Persists pending changes; returns the number of affected rows.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}

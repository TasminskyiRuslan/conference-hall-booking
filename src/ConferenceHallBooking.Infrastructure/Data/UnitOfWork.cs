using ConferenceHallBooking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    /// <summary>Runs the operation in a transaction, joining an outer one when already active.</summary>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        // A nested call must not open a second transaction on the same context
        // (EF Core throws); it runs inside the active one instead, so only the
        // outermost call commits or rolls back.
        if (context.Database.CurrentTransaction is not null)
        {
            await operation(cancellationToken);
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}

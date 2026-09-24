using ConferenceHallBooking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Data;

/// <summary>
/// EF Core implementation of the Unit of Work pattern. Commits all pending changes to the database.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
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

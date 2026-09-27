using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConferenceHallBooking.Infrastructure.Data;

/// <summary>
/// EF Core implementation of the Unit of Work pattern. Commits all pending changes to the database.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <summary>Persists pending changes; returns the number of affected rows.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var postgresException = FindPostgresException(ex);

            if (postgresException?.SqlState == PostgresErrorCodes.ExclusionViolation)
            {
                throw new BookingOverlapException(ex);
            }

            if (postgresException?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new UniqueConstraintViolationException(ex);
            }

            throw;
        }
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }
        }

        return null;
    }
}

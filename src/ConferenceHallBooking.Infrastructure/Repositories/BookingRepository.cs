using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for managing booking data access.
/// </summary>
public class BookingRepository(AppDbContext context) : IBookingRepository
{
    /// <summary>Gets a booking by identifier, or null when it does not exist.</summary>
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .Include(b => b.Hall)
            .Include(b => b.BookingOptions)
                .ThenInclude(bo => bo.Option)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <summary>Checks whether any booking of the hall overlaps the given half-open slot.</summary>
    public async Task<bool> HasOverlappingBookingAsync(
        Guid hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AnyAsync(b => b.HallId == hallId && startTime < b.EndTime && endTime > b.StartTime, cancellationToken);
    }

    /// <summary>Counts all bookings made for the hall.</summary>
    public async Task<int> GetBookingCountByHallIdAsync(
        Guid hallId,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .CountAsync(b => b.HallId == hallId, cancellationToken);
    }

    /// <summary>Gets bookings that start within the given date range.</summary>
    public async Task<IReadOnlyList<Booking>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Include(b => b.Hall)
            .Include(b => b.BookingOptions)
                .ThenInclude(bo => bo.Option)
            .Where(b => b.StartTime < to && b.EndTime > from)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Adds a new booking.</summary>
    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await context.Bookings.AddAsync(booking, cancellationToken);
    }

    /// <summary>Returns options used by upcoming bookings; they cannot be unlinked.</summary>
    public async Task<IReadOnlyCollection<Guid>> GetBlockedOptionIdsForHallAsync(
        Guid hallId,
        IReadOnlyCollection<Guid> candidateOptionIds,
        CancellationToken cancellationToken = default)
    {
        if (candidateOptionIds.Count == 0)
        {
            return [];
        }

        return await context.BookingOptions
            .AsNoTracking()
            .Where(bo => bo.Booking.HallId == hallId
                      && bo.Booking.EndTime > DateTimeOffset.UtcNow
                      && candidateOptionIds.Contains(bo.OptionId))
            .Select(bo => bo.OptionId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}

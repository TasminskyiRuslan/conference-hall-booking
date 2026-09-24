using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for managing hall data access.
/// </summary>
public class HallRepository(AppDbContext context) : IHallRepository
{
    public async Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Halls
            .Include(h => h.HallOptions)
                .ThenInclude(ho => ho.Option)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Hall>> GetAvailableHallsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        var bookedHallIds = await context.Bookings
            .AsNoTracking()
            .Where(b => startTime < b.EndTime && endTime > b.StartTime)
            .Select(b => b.HallId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await context.Halls
            .AsNoTracking()
            .Where(h => h.Capacity >= capacity && !bookedHallIds.Contains(h.Id))
            .Include(h => h.HallOptions)
                .ThenInclude(ho => ho.Option)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Hall>> GetHallsWithBookingsInRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await context.Halls
            .AsNoTracking()
            .Include(h => h.Bookings.Where(b => b.StartTime < to && b.EndTime > from))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Halls.AnyAsync(h => h.Name == name, cancellationToken);
    }

    public async Task AddAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        await context.Halls.AddAsync(hall, cancellationToken);
    }

    public void Delete(Hall hall)
    {
        context.Halls.Remove(hall);
    }
}

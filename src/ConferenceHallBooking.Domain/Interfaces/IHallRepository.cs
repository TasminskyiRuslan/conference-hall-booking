using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing hall data access operations.
/// </summary>
public interface IHallRepository
{
    /// <summary>Gets a hall by identifier, or null when it does not exist.</summary>
    Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Gets halls that are free in the slot and meet the capacity requirement.</summary>
    Task<IReadOnlyList<Hall>> GetAvailableHallsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int capacity,
        CancellationToken cancellationToken = default);

    /// <summary>Gets halls that have any booking within the given range.</summary>
    Task<IReadOnlyList<Hall>> GetHallsWithBookingsInRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    /// <summary>Checks whether a hall with the given name already exists.</summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    /// <summary>Adds a new hall.</summary>
    Task AddAsync(Hall hall, CancellationToken cancellationToken = default);
    /// <summary>Removes the hall from the context.</summary>
    void Delete(Hall hall);
}

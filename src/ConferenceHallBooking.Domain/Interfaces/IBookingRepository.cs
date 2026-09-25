using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing booking data access operations.
/// </summary>
public interface IBookingRepository
{
    /// <summary>Gets a booking by identifier, or null when it does not exist.</summary>
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Checks whether any booking of the hall overlaps the given half-open slot.</summary>
    Task<bool> HasOverlappingBookingAsync(
        Guid hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>Counts all bookings made for the hall.</summary>
    Task<int> GetBookingCountByHallIdAsync(
        Guid hallId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets bookings that start within the given date range.</summary>
    Task<IReadOnlyList<Booking>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a new booking.</summary>
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>Returns options used by upcoming bookings; they cannot be unlinked.</summary>
    Task<IReadOnlyCollection<Guid>> GetBlockedOptionIdsForHallAsync(
        Guid hallId,
        IReadOnlyCollection<Guid> candidateOptionIds,
        CancellationToken cancellationToken = default);
}

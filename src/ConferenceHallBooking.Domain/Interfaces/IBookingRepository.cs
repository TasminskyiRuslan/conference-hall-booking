using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing booking data access operations.
/// </summary>
public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingBookingAsync(
        Guid hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    Task<int> GetBookingCountByHallIdAsync(
        Guid hallId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> GetBlockedOptionIdsForHallAsync(
        Guid hallId,
        IReadOnlyCollection<Guid> candidateOptionIds,
        CancellationToken cancellationToken = default);
}

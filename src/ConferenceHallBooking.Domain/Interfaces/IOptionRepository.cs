using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing service option data access operations.
/// </summary>
public interface IOptionRepository
{
    /// <summary>Gets options for the given IDs, skipping IDs that do not exist.</summary>
    Task<IReadOnlyList<Option>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}

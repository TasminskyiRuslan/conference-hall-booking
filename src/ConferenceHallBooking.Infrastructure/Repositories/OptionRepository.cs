using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for managing option data access.
/// </summary>
public class OptionRepository(AppDbContext context) : IOptionRepository
{
    /// <summary>Gets options for the given IDs, skipping IDs that do not exist.</summary>
    public async Task<IReadOnlyList<Option>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await context.Options
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(cancellationToken);
    }
}

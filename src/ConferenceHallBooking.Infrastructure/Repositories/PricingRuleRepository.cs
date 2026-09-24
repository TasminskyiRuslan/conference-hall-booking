using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for loading pricing rules.
/// </summary>
public class PricingRuleRepository(AppDbContext context) : IPricingRuleRepository
{
    public async Task<IReadOnlyList<PricingRule>> GetOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await context.PricingRules
            .AsNoTracking()
            .OrderBy(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }
}

using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing pricing rule data access operations.
/// </summary>
public interface IPricingRuleRepository
{
    /// <summary>
    /// Returns all pricing rules ordered by start time.
    /// </summary>
    Task<IReadOnlyList<PricingRule>> GetOrderedAsync(CancellationToken cancellationToken = default);
}

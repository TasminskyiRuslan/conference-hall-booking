using ConferenceHallBooking.Application.Common.Models;

namespace ConferenceHallBooking.Application.Interfaces.Bookings;

/// <summary>
/// Calculates booking costs using time-based pricing rules from the database.
/// </summary>
public interface IPricingService
{
    /// <summary>Applies time-of-day multipliers and totals the hall and options cost.</summary>
    Task<PricingResult> CalculatePriceAsync(
        decimal baseHourlyRate,
        IReadOnlyCollection<decimal>? optionPrices,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);
}

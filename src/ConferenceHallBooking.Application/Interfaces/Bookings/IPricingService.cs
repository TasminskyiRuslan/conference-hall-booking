using ConferenceHallBooking.Application.DTOs.Bookings;

namespace ConferenceHallBooking.Application.Interfaces.Bookings;

/// <summary>
/// Calculates booking costs using time-based pricing rules from the database.
/// </summary>
public interface IPricingService
{
    Task<PricingResult> CalculatePriceAsync(
        decimal baseHourlyRate,
        IReadOnlyCollection<decimal>? optionPrices,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);
}

namespace ConferenceHallBooking.Application.DTOs.Bookings;

/// <summary>
/// Calculated pricing breakdown for a booking.
/// </summary>
public record PricingResult(
    decimal HallCost,
    decimal OptionsCost,
    decimal TotalCost);

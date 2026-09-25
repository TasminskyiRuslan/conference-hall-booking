using ConferenceHallBooking.Application.DTOs.Options;

namespace ConferenceHallBooking.Application.DTOs.Bookings;

/// <summary>
/// Response model for a conference hall booking.
/// Cost fields and <see cref="SelectedOptions"/> prices are snapshots
/// taken at booking creation time.
/// </summary>
public record BookingResponse(
    Guid Id,
    Guid HallId,
    string HallName,
    int HallCapacity,
    decimal HallBaseHourlyRate,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal DurationHours,
    IReadOnlyCollection<OptionResponse> SelectedOptions,
    decimal HallCost,
    decimal OptionsCost,
    decimal TotalCost);

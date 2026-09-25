namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>
/// Booking totals, averages and popularity leaders for a period.
/// </summary>
public record BookingSummaryReport(
    DateTimeOffset From,
    DateTimeOffset To,
    int TotalBookings,
    decimal TotalRevenue,
    decimal AverageBookingDurationHours,
    decimal AverageBookingRevenue,
    IReadOnlyCollection<PopularTimeSlot> PopularTimeSlots,
    IReadOnlyCollection<PopularOption> PopularOptions);

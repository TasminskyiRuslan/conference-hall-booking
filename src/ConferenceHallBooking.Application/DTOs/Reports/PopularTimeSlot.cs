namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>Booking count for a frequently used hour of day.</summary>
public record PopularTimeSlot(
    int Hour,
    int BookingCount);

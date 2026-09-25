namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>Booking count for a frequently requested service option.</summary>
public record PopularOption(
    Guid OptionId,
    string Name,
    int BookingCount);

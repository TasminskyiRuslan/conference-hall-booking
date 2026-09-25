namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>Revenue attributed to a single hall within the period.</summary>
public record HallRevenue(
    Guid HallId,
    string HallName,
    int BookingCount,
    decimal Revenue);

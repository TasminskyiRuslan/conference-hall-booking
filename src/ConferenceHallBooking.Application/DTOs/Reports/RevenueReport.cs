namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>
/// Revenue for a period, broken down by hall.
/// </summary>
public record RevenueReport(
    DateTimeOffset From,
    DateTimeOffset To,
    decimal TotalRevenue,
    IReadOnlyCollection<HallRevenue> ByHall);

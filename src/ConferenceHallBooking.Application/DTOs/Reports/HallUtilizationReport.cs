namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>
/// Booked vs available hours for all halls in a period.
/// </summary>
public record HallUtilizationReport(
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyCollection<HallUtilization> Halls);

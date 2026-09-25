namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>Utilization figures for a single hall within the period.</summary>
public record HallUtilization(
    Guid HallId,
    string HallName,
    int Capacity,
    decimal BookedHours,
    decimal AvailableHours,
    decimal UtilizationPercent);

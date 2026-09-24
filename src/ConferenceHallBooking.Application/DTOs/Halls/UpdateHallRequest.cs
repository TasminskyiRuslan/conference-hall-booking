namespace ConferenceHallBooking.Application.DTOs.Halls;

/// <summary>
/// Request model for updating an existing conference hall.
/// </summary>
public record UpdateHallRequest(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid>? OptionIds);

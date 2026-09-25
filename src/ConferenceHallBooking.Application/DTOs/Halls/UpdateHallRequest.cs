namespace ConferenceHallBooking.Application.DTOs.Halls;

/// <summary>
/// Request model for updating an existing conference hall.
/// Body of the update endpoint; the hall Id comes from the route
/// and is mapped into the command by the controller.
/// </summary>
public record UpdateHallRequest(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid>? OptionIds);

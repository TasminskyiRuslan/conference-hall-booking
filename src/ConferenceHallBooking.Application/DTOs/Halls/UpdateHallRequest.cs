namespace ConferenceHallBooking.Application.DTOs.Halls;

/// <summary>
/// Request model for updating an existing conference hall.
/// The hall Id is a separate value supplied outside this payload.
/// </summary>
public record UpdateHallRequest(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid>? OptionIds);

using ConferenceHallBooking.Application.DTOs.Options;

namespace ConferenceHallBooking.Application.DTOs.Halls;

/// <summary>
/// Response model for a conference hall.
/// </summary>
public record HallResponse(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    IReadOnlyCollection<OptionResponse> Options);

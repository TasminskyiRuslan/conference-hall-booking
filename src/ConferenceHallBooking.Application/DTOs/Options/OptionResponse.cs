namespace ConferenceHallBooking.Application.DTOs.Options;

/// <summary>
/// Response model for a service option.
/// </summary>
public record OptionResponse(
    Guid Id,
    string Name,
    decimal Price);

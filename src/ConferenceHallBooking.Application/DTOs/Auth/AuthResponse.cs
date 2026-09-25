namespace ConferenceHallBooking.Application.DTOs.Auth;

/// <summary>
/// Authentication result containing the JWT token.
/// </summary>
public record AuthResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string Token);

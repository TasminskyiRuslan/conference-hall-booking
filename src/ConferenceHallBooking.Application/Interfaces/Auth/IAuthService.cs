using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;

namespace ConferenceHallBooking.Application.Interfaces.Auth;

/// <summary>
/// Handles user registration, authentication, and JWT token generation.
/// </summary>
public interface IAuthService
{
    /// <summary>Creates the account and returns an auth token.</summary>
    Task<AuthResponse> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);
    /// <summary>Authenticates credentials and returns an auth token.</summary>
    Task<AuthResponse> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
}

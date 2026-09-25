using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;

namespace ConferenceHallBooking.Application.Interfaces.Auth;

/// <summary>
/// Handles user registration, authentication, and JWT token generation.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
}

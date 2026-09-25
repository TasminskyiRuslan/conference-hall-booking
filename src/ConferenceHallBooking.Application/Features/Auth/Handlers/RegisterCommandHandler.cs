using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Interfaces.Auth;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Auth.Handlers;

/// <summary>
/// Handler for RegisterCommand. Delegates to IAuthService.
/// </summary>
public class RegisterCommandHandler(IAuthService authService)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    /// <summary>Delegates account creation to IAuthService and returns the token response.</summary>
    public async Task<AuthResponse> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        return await authService.RegisterAsync(request, cancellationToken);
    }
}

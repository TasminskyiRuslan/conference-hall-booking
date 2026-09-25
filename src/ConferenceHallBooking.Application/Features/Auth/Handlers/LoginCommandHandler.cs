using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Interfaces.Auth;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Auth.Handlers;

/// <summary>
/// Handler for LoginCommand. Delegates to IAuthService.
/// </summary>
public class LoginCommandHandler(IAuthService authService)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.LoginAsync(request, cancellationToken);
    }
}

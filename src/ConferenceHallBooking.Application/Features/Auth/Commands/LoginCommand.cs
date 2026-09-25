using ConferenceHallBooking.Application.DTOs.Auth;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Auth.Commands;

/// <summary>
/// Command to authenticate a user and issue a JWT token.
/// </summary>
public record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResponse>;

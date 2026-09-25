using ConferenceHallBooking.Application.DTOs.Auth;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Auth.Commands;

/// <summary>
/// Command to register a new user account and issue a JWT token.
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName) : IRequest<AuthResponse>;

using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Features.Auth.Handlers;
using ConferenceHallBooking.Application.Interfaces.Auth;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Auth;

public class AuthCommandHandlerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();

    [Fact]
    public async Task RegisterCommandHandler_ShouldDelegateToAuthService_AndReturnResponse()
    {
        var command = new RegisterCommand(
            "test@example.com", "Passw0rd!", "Passw0rd!", "Test User");
        var response = new AuthResponse(
            Guid.NewGuid(), "test@example.com", "Test User", "Visitor", "jwt-token");
        _authService.RegisterAsync(command, Arg.Any<CancellationToken>()).Returns(response);
        var handler = new RegisterCommandHandler(_authService);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(response);
        await _authService.Received(1).RegisterAsync(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterCommandHandler_WhenAuthServiceThrows_ShouldPropagate()
    {
        var command = new RegisterCommand(
            "taken@example.com", "Passw0rd!", "Passw0rd!", "Test User");
        _authService.RegisterAsync(command, Arg.Any<CancellationToken>())
            .Returns<AuthResponse>(_ => throw new EmailAlreadyExistsException(command.Email));
        var handler = new RegisterCommandHandler(_authService);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
    }

    [Fact]
    public async Task LoginCommandHandler_ShouldDelegateToAuthService_AndReturnResponse()
    {
        var command = new LoginCommand("test@example.com", "Passw0rd!");
        var response = new AuthResponse(
            Guid.NewGuid(), "test@example.com", "Test User", "Visitor", "jwt-token");
        _authService.LoginAsync(command, Arg.Any<CancellationToken>()).Returns(response);
        var handler = new LoginCommandHandler(_authService);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(response);
        await _authService.Received(1).LoginAsync(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginCommandHandler_WhenAuthServiceThrows_ShouldPropagate()
    {
        var command = new LoginCommand("test@example.com", "wrong");
        _authService.LoginAsync(command, Arg.Any<CancellationToken>())
            .Returns<AuthResponse>(_ => throw new InvalidCredentialsException());
        var handler = new LoginCommandHandler(_authService);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}

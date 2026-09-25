using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Domain.Common;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Services;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOptions<JwtSettings> _jwtSettings = Options.Create(new JwtSettings
    {
        SecretKey = "TestSecretKeyThatIsAtLeast32CharactersLong!",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpirationInMinutes = 60
    });

    private AuthService CreateService() => new(_userRepository, _unitOfWork, _jwtSettings);

    #region Register

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnToken()
    {
        _userRepository.ExistsByEmailAsync("new@email.com", Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await CreateService().RegisterAsync(
            new RegisterCommand("new@email.com", "password123", "password123", "John Doe"));

        result.UserId.Should().NotBe(Guid.Empty);
        result.Email.Should().Be("new@email.com");
        result.FullName.Should().Be("John Doe");
        result.Token.Should().NotBeNullOrEmpty();
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowEmailAlreadyExistsException()
    {
        _userRepository.ExistsByEmailAsync("existing@email.com", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await CreateService().RegisterAsync(
            new RegisterCommand("existing@email.com", "password123", "password123", "John"));

        await act.Should().ThrowAsync<EmailAlreadyExistsException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        _userRepository.ExistsByEmailAsync("new@email.com", Arg.Any<CancellationToken>())
            .Returns(false);

        await CreateService().RegisterAsync(
            new RegisterCommand("new@email.com", "myPassword123", "myPassword123", "John"));

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.PasswordHash != "myPassword123" && u.PasswordHash.StartsWith("$2a$")),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Login

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword");
        var user = new User("test@email.com", passwordHash, "John Doe");
        _userRepository.GetByEmailAsync("test@email.com", Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await CreateService().LoginAsync(
            new LoginCommand("test@email.com", "correctPassword"));

        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("test@email.com");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword");
        var user = new User("test@email.com", passwordHash, "John Doe");
        _userRepository.GetByEmailAsync("test@email.com", Arg.Any<CancellationToken>())
            .Returns(user);

        var act = async () => await CreateService().LoginAsync(
            new LoginCommand("test@email.com", "wrongPassword"));

        await act.Should().ThrowAsync<InvalidCredentialsException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowInvalidCredentialsException()
    {
        _userRepository.GetByEmailAsync("nonexistent@email.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = async () => await CreateService().LoginAsync(
            new LoginCommand("nonexistent@email.com", "password"));

        await act.Should().ThrowAsync<InvalidCredentialsException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUserRole()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("pass");
        var user = new User("admin@email.com", passwordHash, "Admin User", UserRole.Admin);
        _userRepository.GetByEmailAsync("admin@email.com", Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await CreateService().LoginAsync(
            new LoginCommand("admin@email.com", "pass"));

        result.Role.Should().Be("Admin");
    }

    #endregion

    #region JWT Token

    [Fact]
    public async Task RegisterAsync_ShouldReturnValidJwtToken()
    {
        _userRepository.ExistsByEmailAsync("new@email.com", Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await CreateService().RegisterAsync(
            new RegisterCommand("new@email.com", "password123", "password123", "John Doe"));

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_jwtSettings.Value.SecretKey));
        var validationParams = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = _jwtSettings.Value.Issuer,
            ValidAudience = _jwtSettings.Value.Audience,
            IssuerSigningKey = key,
            ValidateLifetime = false
        };

        var principal = handler.ValidateToken(result.Token, validationParams, out _);

        principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            .Should().Be("new@email.com");
        principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
            .Should().Be("John Doe");
        principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            .Should().Be("Customer");
    }

    #endregion
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Interfaces.Auth;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConferenceHallBooking.Infrastructure.Services;

/// <summary>
/// Implements JWT-based authentication with user registration and login.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings) : IAuthService
{
    private readonly SecurityKey _signingKey =
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.SecretKey));

    public async Task<AuthResponse> RegisterAsync(
        RegisterCommand command, CancellationToken cancellationToken = default)
    {
        if (await userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            throw new EmailAlreadyExistsException(command.Email);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        var user = new User(command.Email, passwordHash, command.FullName);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = GenerateJwtToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString(), token);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = GenerateJwtToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString(), token);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.Value.ExpirationInMinutes),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

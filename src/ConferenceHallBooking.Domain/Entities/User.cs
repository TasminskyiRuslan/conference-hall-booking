using ConferenceHallBooking.Domain.Common;
using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>System user with credentials and role.</summary>
public class User
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Email used for login (unique).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>BCrypt-hashed password.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Display name.</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Authorization role.</summary>
    public UserRole Role { get; private set; } = UserRole.Customer;

    /// <summary>UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private User() { }

    public User(string email, string passwordHash, string fullName, UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidEntityFieldException(nameof(User), nameof(Email), "email cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new InvalidEntityFieldException(nameof(User), nameof(PasswordHash), "password hash cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidEntityFieldException(nameof(User), nameof(FullName), "full name cannot be empty");
        }

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}

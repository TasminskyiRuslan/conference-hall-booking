using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for user authentication and management operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>Gets a user by email for login, or null.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    /// <summary>Gets a user by identifier, or null.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Checks whether the email is already registered.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    /// <summary>Adds a new user.</summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}

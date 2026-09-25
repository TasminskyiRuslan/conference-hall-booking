namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when registering a user with an email that already exists.
/// </summary>
public class EmailAlreadyExistsException(string email)
    : BusinessRuleException(
        $"A user with the email '{email}' already exists.",
        "EMAIL_ALREADY_EXISTS")
{
    /// <summary>Email that is already registered.</summary>
    public string Email { get; } = email;
}

namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when login credentials (email or password) are invalid.
/// Uses a generic message to prevent email enumeration attacks.
/// </summary>
public class InvalidCredentialsException()
    : BusinessRuleException(
        "Invalid email or password.",
        "INVALID_CREDENTIALS");

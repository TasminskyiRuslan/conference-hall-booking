namespace ConferenceHallBooking.Domain.Exceptions;

public class EmailAlreadyExistsException(string email)
    : BusinessRuleException(
        $"A user with the email '{email}' already exists.",
        "EMAIL_ALREADY_EXISTS")
{
    public string Email { get; } = email;
}

namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when a persistence layer rejects a write because a referenced
/// entity no longer exists (concurrent delete won the race on a foreign key).
/// </summary>
public class ForeignKeyConstraintViolationException(Exception innerException)
    : BusinessRuleException(
        "A related resource required by this operation no longer exists.",
        "FOREIGN_KEY_VIOLATION",
        innerException);

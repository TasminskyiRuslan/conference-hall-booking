namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when a unique constraint is violated
/// (e.g. concurrent duplicate registration).
/// </summary>
public class UniqueConstraintViolationException(Exception innerException)
    : BusinessRuleException(
        "A resource with the same unique constraint already exists.",
        "UNIQUE_CONSTRAINT_VIOLATION",
        innerException);

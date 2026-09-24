namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when a user attempts to access a booking they do not own
/// and are not an admin.
/// Uses a generic message to avoid leaking booking existence.
/// </summary>
public class BookingAccessException()
    : BusinessRuleException(
        "You do not have permission to access this booking.",
        "BOOKING_ACCESS_DENIED");

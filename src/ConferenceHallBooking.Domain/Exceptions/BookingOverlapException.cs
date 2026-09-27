namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when the persistence layer rejects overlapping bookings
/// (concurrent booking won the race on the exclusion constraint).
/// </summary>
public class BookingOverlapException(Exception innerException)
    : BusinessRuleException(
        "The hall is already booked for an overlapping time slot.",
        "HALL_ALREADY_BOOKED",
        innerException);

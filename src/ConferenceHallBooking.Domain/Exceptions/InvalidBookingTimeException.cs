namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when a booking's end time is not after its start time.
/// </summary>
public class InvalidBookingTimeException(DateTimeOffset startTime, DateTimeOffset endTime)
    : BusinessRuleException(
        $"Booking end time ({endTime:yyyy-MM-dd HH:mm}) must be after start time ({startTime:yyyy-MM-dd HH:mm}).",
        "INVALID_BOOKING_TIME");

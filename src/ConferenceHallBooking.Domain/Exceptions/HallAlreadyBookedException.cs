namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when a hall is already booked for the requested time slot.
/// </summary>
public class HallAlreadyBookedException(Guid hallId, DateTimeOffset startTime, DateTimeOffset endTime)
    : BusinessRuleException(
        $"Hall {hallId} is already booked from {startTime:yyyy-MM-dd HH:mm} to {endTime:yyyy-MM-dd HH:mm}.",
        "HALL_ALREADY_BOOKED");

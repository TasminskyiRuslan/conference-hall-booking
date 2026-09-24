namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to delete a hall that has existing bookings.
/// </summary>
public class HallHasBookingsException(Guid hallId, int bookingCount)
    : BusinessRuleException(
        $"Hall {hallId} has {bookingCount} existing booking(s) and cannot be deleted.",
        "HALL_HAS_BOOKINGS")
{
    public Guid HallId { get; } = hallId;
    public int BookingCount { get; } = bookingCount;
}

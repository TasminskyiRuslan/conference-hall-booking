namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to delete a hall that has existing bookings.
/// </summary>
public class HallHasBookingsException(Guid hallId, int bookingCount)
    : BusinessRuleException(
        $"Hall {hallId} has {bookingCount} existing booking(s) and cannot be deleted.",
        "HALL_HAS_BOOKINGS")
{
    /// <summary>Hall that could not be deleted.</summary>
    public Guid HallId { get; } = hallId;
    /// <summary>Number of bookings that blocked the deletion.</summary>
    public int BookingCount { get; } = bookingCount;
}

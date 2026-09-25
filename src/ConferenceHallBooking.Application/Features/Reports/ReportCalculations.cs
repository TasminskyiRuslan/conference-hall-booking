using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Application.Features.Reports;

/// <summary>
/// Shared calculations for report handlers.
/// </summary>
public static class ReportCalculations
{
    /// <summary>
    /// Splits the total price proportionally to the part of the booking
    /// that falls inside [from, to). Returns zero when there is no overlap.
    /// </summary>
    public static decimal ProportionalRevenue(Booking booking, DateTimeOffset from, DateTimeOffset to)
    {
        var overlapStart = booking.StartTime < from ? from : booking.StartTime;
        var overlapEnd = booking.EndTime > to ? to : booking.EndTime;
        var overlapHours = (decimal)(overlapEnd - overlapStart).TotalHours;
        var totalHours = (decimal)(booking.EndTime - booking.StartTime).TotalHours;

        if (totalHours <= 0 || overlapHours <= 0)
        {
            return 0;
        }

        return booking.TotalPrice * overlapHours / totalHours;
    }
}

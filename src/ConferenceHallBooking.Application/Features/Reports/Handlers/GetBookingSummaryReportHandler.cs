using ConferenceHallBooking.Application.DTOs.Reports;
using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Reports.Handlers;

/// <summary>
/// Handler for <see cref="GetBookingSummaryReportQuery"/>.
/// </summary>
public class GetBookingSummaryReportHandler(IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingSummaryReportQuery, BookingSummaryReport>
{
    /// <summary>Sums totals, averages and popularity leaders for bookings in the period.</summary>
    public async Task<BookingSummaryReport> Handle(
        GetBookingSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetByDateRangeAsync(
            request.From, request.To, cancellationToken);

        var totalBookings = bookings.Count;
        var totalRevenue = bookings.Sum(b => ReportCalculations.ProportionalRevenue(b, request.From, request.To));

        var avgDuration = totalBookings > 0
            ? Math.Round(bookings.Average(b => (b.EndTime - b.StartTime).TotalHours), 2)
            : 0;

        var avgRevenue = totalBookings > 0
            ? Math.Round(totalRevenue / totalBookings, 2)
            : 0;

        var popularTimeSlots = bookings
            .GroupBy(b => b.StartTime.Hour)
            .Select(g => new PopularTimeSlot(g.Key, g.Count()))
            .OrderByDescending(s => s.BookingCount)
            .Take(10)
            .ToList();

        var popularOptions = bookings
            .SelectMany(b => b.BookingOptions)
            .GroupBy(bo => new { bo.OptionId, bo.Option.Name })
            .Select(g => new PopularOption(g.Key.OptionId, g.Key.Name, g.Count()))
            .OrderByDescending(s => s.BookingCount)
            .Take(10)
            .ToList();

        return new BookingSummaryReport(
            request.From, request.To,
            totalBookings,
            totalRevenue,
            (decimal)avgDuration,
            (decimal)avgRevenue,
            popularTimeSlots,
            popularOptions);
    }
}

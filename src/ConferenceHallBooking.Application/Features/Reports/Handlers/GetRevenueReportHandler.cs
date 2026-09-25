using ConferenceHallBooking.Application.DTOs.Reports;
using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Reports.Handlers;

/// <summary>
/// Handler for <see cref="GetRevenueReportQuery"/>.
/// </summary>
public class GetRevenueReportHandler(IBookingRepository bookingRepository)
    : IRequestHandler<GetRevenueReportQuery, RevenueReport>
{
    /// <summary>Sums proportional revenue per hall for bookings overlapping the period.</summary>
    public async Task<RevenueReport> Handle(
        GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetByDateRangeAsync(
            request.From, request.To, cancellationToken);

        var totalRevenue = bookings.Sum(b => ReportCalculations.ProportionalRevenue(b, request.From, request.To));

        var byHall = bookings
            .GroupBy(b => new { b.HallId, b.Hall.Name })
            .Select(g => new HallRevenue(
                g.Key.HallId,
                g.Key.Name,
                g.Count(),
                g.Sum(b => ReportCalculations.ProportionalRevenue(b, request.From, request.To))))
            .OrderByDescending(h => h.Revenue)
            .ToList();

        return new RevenueReport(request.From, request.To, totalRevenue, byHall);
    }
}

using ConferenceHallBooking.Application.DTOs.Reports;
using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Reports.Handlers;

/// <summary>
/// Handler for <see cref="GetHallUtilizationReportQuery"/>.
/// </summary>
public class GetHallUtilizationReportHandler(IHallRepository hallRepository)
    : IRequestHandler<GetHallUtilizationReportQuery, HallUtilizationReport>
{
    /// <summary>Computes booked vs available hours per hall for the period.</summary>
    public async Task<HallUtilizationReport> Handle(
        GetHallUtilizationReportQuery request, CancellationToken cancellationToken)
    {
        var halls = await hallRepository.GetHallsWithBookingsInRangeAsync(
            request.From, request.To, cancellationToken);

        var periodHours = (decimal)(request.To - request.From).TotalHours;

        var hallUtilizations = halls.Select(h =>
        {
            var bookedHours = h.Bookings.Sum(b =>
            {
                var bookingStart = b.StartTime < request.From ? request.From : b.StartTime;
                var bookingEnd = b.EndTime > request.To ? request.To : b.EndTime;
                return (decimal)(bookingEnd - bookingStart).TotalHours;
            });

            bookedHours = Math.Max(0, Math.Min(bookedHours, periodHours));
            var utilizationPercent = periodHours > 0
                ? Math.Round(bookedHours / periodHours * 100, 1)
                : 0;

            return new HallUtilization(
                h.Id,
                h.Name,
                h.Capacity,
                Math.Round(bookedHours, 2),
                Math.Round(Math.Max(0, periodHours - bookedHours), 2),
                utilizationPercent);
        })
        .OrderByDescending(h => h.UtilizationPercent)
        .ToList();

        return new HallUtilizationReport(request.From, request.To, hallUtilizations);
    }
}

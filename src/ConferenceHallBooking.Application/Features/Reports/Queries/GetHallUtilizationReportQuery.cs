using ConferenceHallBooking.Application.DTOs.Reports;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Reports.Queries;

/// <summary>
/// Queries hall utilization statistics (booked vs available hours) for a given time period.
/// </summary>
public record GetHallUtilizationReportQuery(
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<HallUtilizationReport>;

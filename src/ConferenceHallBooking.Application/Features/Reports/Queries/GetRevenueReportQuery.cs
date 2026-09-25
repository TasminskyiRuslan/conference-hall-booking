using ConferenceHallBooking.Application.DTOs.Reports;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Reports.Queries;

/// <summary>
/// Queries revenue breakdown by hall for a given time period.
/// </summary>
public record GetRevenueReportQuery(
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<RevenueReport>;

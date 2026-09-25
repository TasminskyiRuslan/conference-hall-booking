using ConferenceHallBooking.Application.DTOs.Reports;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Reports.Queries;

/// <summary>
/// Queries overall booking summary with popular time slots for a given time period.
/// </summary>
public record GetBookingSummaryReportQuery(
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<BookingSummaryReport>;

using ConferenceHallBooking.Application.DTOs.Reports;
using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHallBooking.Api.Controllers;

/// <summary>
/// Provides business analytics and reporting endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReportController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Gets revenue breakdown by hall for a specified time period.
    /// </summary>
    [HttpGet("revenue")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(RevenueReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var report = await sender.Send(new GetRevenueReportQuery(from, to), cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Gets hall utilization statistics (booked vs available hours) for a specified time period.
    /// </summary>
    [HttpGet("utilization")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(HallUtilizationReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUtilization(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var report = await sender.Send(new GetHallUtilizationReportQuery(from, to), cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Gets overall booking summary with popular time slots for a specified time period.
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(BookingSummaryReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var report = await sender.Send(new GetBookingSummaryReportQuery(from, to), cancellationToken);
        return Ok(report);
    }
}

using ConferenceHallBooking.Api.Extensions;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Features.Bookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHallBooking.Api.Controllers;

/// <summary>
/// Manages conference hall bookings — create and retrieve.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class BookingController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Gets a booking by its ID. Only the booking owner and administrators may view it.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var booking = await sender.Send(new GetBookingByIdQuery(id, userId), cancellationToken);
        return Ok(booking);
    }

    /// <summary>
    /// Books a conference hall for a specified time slot with optional services.
    /// The booking is always attributed to the authenticated user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var booking = await sender.Send(command with { UserId = userId }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }
}

using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHallBooking.Api.Controllers;

/// <summary>
/// Manages conference halls — create, update, delete, search, and retrieve.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class HallController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Gets a conference hall by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HallResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var hall = await sender.Send(new GetHallByIdQuery(id), cancellationToken);
        return Ok(hall);
    }

    /// <summary>
    /// Creates a new conference hall.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(HallResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateHallCommand command,
        CancellationToken cancellationToken)
    {
        var hall = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = hall.Id }, hall);
    }

    /// <summary>
    /// Updates an existing conference hall.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(HallResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateHallRequest body,
        CancellationToken cancellationToken)
    {
        var hall = await sender.Send(
            new UpdateHallCommand(id, body.Name, body.Capacity, body.BaseHourlyRate, body.OptionIds),
            cancellationToken);
        return Ok(hall);
    }

    /// <summary>
    /// Deletes a conference hall.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteHallCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Searches for available conference halls by date, time, and capacity.
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyCollection<HallResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] SearchAvailableHallsQuery query,
        CancellationToken cancellationToken)
    {
        var halls = await sender.Send(query, cancellationToken);
        return Ok(halls);
    }
}

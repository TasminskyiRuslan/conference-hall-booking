using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Commands;

/// <summary>
/// Command to delete a conference hall. Fails if the hall has existing bookings.
/// </summary>
public record DeleteHallCommand(Guid Id) : IRequest;

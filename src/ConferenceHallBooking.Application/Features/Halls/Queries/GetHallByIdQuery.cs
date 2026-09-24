using ConferenceHallBooking.Application.DTOs.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Queries;

/// <summary>
/// Query to get a single conference hall by its ID.
/// </summary>
public record GetHallByIdQuery(Guid Id) : IRequest<HallResponse>;

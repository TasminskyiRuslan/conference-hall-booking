using ConferenceHallBooking.Application.DTOs.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Queries;

/// <summary>
/// Query that finds halls available for a given time slot with a minimum capacity.
/// </summary>
public record SearchAvailableHallsQuery(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int Capacity) : IRequest<IReadOnlyCollection<HallResponse>>;

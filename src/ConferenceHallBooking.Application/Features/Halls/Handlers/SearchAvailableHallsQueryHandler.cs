using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

/// <summary>
/// Handler for SearchAvailableHallsQuery.
/// </summary>
public class SearchAvailableHallsQueryHandler(IHallRepository hallRepository)
    : IRequestHandler<SearchAvailableHallsQuery, IReadOnlyCollection<HallResponse>>
{
    public async Task<IReadOnlyCollection<HallResponse>> Handle(
        SearchAvailableHallsQuery request,
        CancellationToken cancellationToken)
    {
        var halls = await hallRepository.GetAvailableHallsAsync(
            request.StartTime,
            request.EndTime,
            request.Capacity,
            cancellationToken);

        return halls.Select(HallMapper.MapToResponse).ToList().AsReadOnly();
    }
}

using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

/// <summary>Handler for GetHallByIdQuery. Returns the hall with its options.</summary>
public class GetHallByIdQueryHandler(IHallRepository hallRepository)
    : IRequestHandler<GetHallByIdQuery, HallResponse>
{
    /// <summary>Returns a hall with its linked options.</summary>
    public async Task<HallResponse> Handle(GetHallByIdQuery request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.Id.ToString());

        return HallMapper.MapToResponse(hall);
    }
}

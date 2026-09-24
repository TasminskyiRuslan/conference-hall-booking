using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

/// <summary>
/// Handler for CreateHallCommand.
/// </summary>
public class CreateHallCommandHandler(
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateHallCommand, HallResponse>
{
    public async Task<HallResponse> Handle(CreateHallCommand request, CancellationToken cancellationToken)
    {
        if (await hallRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new HallNameAlreadyExistsException(request.Name);
        }

        var hall = new Hall(request.Name, request.Capacity, request.BaseHourlyRate);

        if (request.OptionIds is { Count: > 0 })
        {
            var linkedOptions = await optionRepository.GetByIdsOrThrowAsync(request.OptionIds, cancellationToken);

            foreach (var option in linkedOptions)
            {
                hall.AddOption(option);
            }
        }

        await hallRepository.AddAsync(hall, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return HallMapper.MapToResponse(hall);
    }
}

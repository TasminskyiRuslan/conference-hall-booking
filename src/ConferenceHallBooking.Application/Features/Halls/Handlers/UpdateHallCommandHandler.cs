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
/// Handler for UpdateHallCommand.
/// </summary>
public class UpdateHallCommandHandler(
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateHallCommand, HallResponse>
{
    public async Task<HallResponse> Handle(UpdateHallCommand request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.Id.ToString());

        if (hall.Name != request.Name && await hallRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new HallNameAlreadyExistsException(request.Name);
        }

        hall.Update(request.Name, request.Capacity, request.BaseHourlyRate);

        await SynchronizeOptionsAsync(hall, request.OptionIds, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return HallMapper.MapToResponse(hall);
    }

    private async Task SynchronizeOptionsAsync(Hall hall, List<Guid>? targetOptionIds, CancellationToken cancellationToken)
    {
        IReadOnlyList<Option> targetOptions = targetOptionIds is { Count: > 0 }
            ? await optionRepository.GetByIdsOrThrowAsync(targetOptionIds, cancellationToken)
            : [];

        var desiredIds = targetOptions.Select(o => o.Id).ToHashSet();
        var currentIds = hall.HallOptions.Select(ho => ho.OptionId).ToHashSet();

        var idsToRemove = currentIds.Except(desiredIds).ToList();
        if (idsToRemove.Count > 0)
        {
            var blockedIds = await bookingRepository.GetBlockedOptionIdsForHallAsync(
                hall.Id, idsToRemove, cancellationToken);
            if (blockedIds.Count > 0)
            {
                throw new HallOptionInUseException(hall.Id, blockedIds);
            }

            foreach (var id in idsToRemove)
            {
                hall.RemoveOption(id);
            }
        }

        foreach (var option in targetOptions.Where(o => !currentIds.Contains(o.Id)))
        {
            hall.AddOption(option);
        }
    }
}

using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

/// <summary>
/// Handler for DeleteHallCommand.
/// </summary>
public class DeleteHallCommandHandler(
    IHallRepository hallRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteHallCommand>
{
    public async Task Handle(DeleteHallCommand request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.Id.ToString());

        var bookingCount = await bookingRepository.GetBookingCountByHallIdAsync(hall.Id, cancellationToken);

        if (bookingCount > 0)
        {
            throw new HallHasBookingsException(hall.Id, bookingCount);
        }

        hallRepository.Delete(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

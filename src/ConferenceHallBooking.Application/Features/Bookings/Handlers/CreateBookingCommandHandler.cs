using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Handlers;

/// <summary>
/// Handler for CreateBookingCommand. Overlaps are rejected by the pre-check
/// and race-safe by the database EXCLUDE constraint.
/// </summary>
public class CreateBookingCommandHandler(
    IBookingRepository bookingRepository,
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IUserRepository userRepository,
    IPricingService pricingService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    /// <summary>Checks hall, user, overlap and options, then persists a priced booking.</summary>
    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.HallId, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.HallId.ToString());

        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException<User>(request.UserId.ToString());

        var startTime = request.StartTime;
        var endTime = startTime.AddHours((double)request.DurationHours);

        if (await bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, cancellationToken))
        {
            throw new HallAlreadyBookedException(hall.Id, startTime, endTime);
        }

        var optionIds = new HashSet<Guid>(request.OptionIds ?? []);

        var allowedOptionIds = hall.HallOptions.Select(ho => ho.OptionId).ToHashSet();

        var unsupportedOptionIds = optionIds.Where(id => !allowedOptionIds.Contains(id)).ToList();

        if (unsupportedOptionIds.Count != 0)
        {
            throw new HallOptionNotSupportedException(hall.Id, unsupportedOptionIds);
        }

        var selectedOptions = await optionRepository.GetByIdsOrThrowAsync(optionIds, cancellationToken);

        var pricing = await pricingService.CalculatePriceAsync(
            hall.BaseHourlyRate,
            selectedOptions.Select(o => o.Price).ToList(),
            startTime,
            endTime,
            cancellationToken);

        var bookingOptions = selectedOptions.Select(o => new BookingOption(o, o.Price));
        var booking = new Booking(hall, user, startTime, endTime, pricing.HallCost, pricing.TotalCost, bookingOptions);

        await bookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingMapper.MapToResponse(booking);
    }
}

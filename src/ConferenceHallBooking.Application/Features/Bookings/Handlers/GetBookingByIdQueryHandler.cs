using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Features.Bookings.Queries;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Common;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Handlers;

/// <summary>
/// Handler for GetBookingByIdQuery. Access is limited to the booking owner
/// and admins; denial throws a generic error to avoid leaking booking existence.
/// </summary>
public class GetBookingByIdQueryHandler(
    IBookingRepository bookingRepository,
    IUserRepository userRepository) : IRequestHandler<GetBookingByIdQuery, BookingResponse>
{
    public async Task<BookingResponse> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Booking>(request.Id.ToString());

        if (booking.UserId != request.CurrentUserId)
        {
            var currentUser = await userRepository.GetByIdAsync(request.CurrentUserId, cancellationToken);

            if (currentUser?.Role != UserRole.Admin)
            {
                throw new BookingAccessException();
            }
        }

        return BookingMapper.MapToResponse(booking);
    }
}

using ConferenceHallBooking.Application.DTOs.Bookings;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Queries;

/// <summary>
/// Query to get a single booking by its ID on behalf of a user.
/// </summary>
public record GetBookingByIdQuery(Guid Id, Guid CurrentUserId) : IRequest<BookingResponse>;

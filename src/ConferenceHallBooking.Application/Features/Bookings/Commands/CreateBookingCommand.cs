using ConferenceHallBooking.Application.DTOs.Bookings;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Commands;

/// <summary>
/// Command to book a conference hall for a given duration with optional services.
/// </summary>
public record CreateBookingCommand(
    Guid HallId,
    Guid UserId,
    DateTimeOffset StartTime,
    decimal DurationHours,
    List<Guid>? OptionIds) : IRequest<BookingResponse>;

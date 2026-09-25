using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Application.Mappers;

/// <summary>
/// Maps Booking domain entities to BookingResponse DTOs.
/// Selected options and hall are read from navigation properties,
/// set at construction or loaded via Include.
/// </summary>
public static class BookingMapper
{
    public static BookingResponse MapToResponse(Booking booking)
    {
        var selectedOptions = booking.BookingOptions
            .Select(bo => new OptionResponse(bo.OptionId, bo.Option.Name, bo.PriceAtBooking))
            .ToList();

        var durationHours = (decimal)(booking.EndTime - booking.StartTime).TotalHours;

        return new BookingResponse(
            booking.Id,
            booking.HallId,
            booking.Hall.Name,
            booking.Hall.Capacity,
            booking.Hall.BaseHourlyRate,
            booking.StartTime,
            booking.EndTime,
            durationHours,
            selectedOptions,
            booking.HallCost,
            selectedOptions.Sum(o => o.Price),
            booking.TotalPrice);
    }
}

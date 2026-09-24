using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Application.Mappers;

/// <summary>
/// Maps Hall domain entities to HallResponse DTOs.
/// </summary>
public static class HallMapper
{
    public static HallResponse MapToResponse(Hall hall)
    {
        var optionResponses = hall.HallOptions
            .Select(ho => new OptionResponse(ho.OptionId, ho.Option.Name, ho.Option.Price))
            .ToList();

        return new HallResponse(
            hall.Id,
            hall.Name,
            hall.Capacity,
            hall.BaseHourlyRate,
            optionResponses);
    }
}

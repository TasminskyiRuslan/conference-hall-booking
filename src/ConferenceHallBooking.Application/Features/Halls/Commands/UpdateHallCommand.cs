using ConferenceHallBooking.Application.DTOs.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Commands;

/// <summary>
/// Command to update an existing conference hall and synchronize its service options.
/// </summary>
public record UpdateHallCommand(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid>? OptionIds) : IRequest<HallResponse>;

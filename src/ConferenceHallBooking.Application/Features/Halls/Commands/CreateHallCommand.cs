using ConferenceHallBooking.Application.DTOs.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Commands;

/// <summary>
/// Command to create a new conference hall with optional service options.
/// </summary>
public record CreateHallCommand(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid>? OptionIds) : IRequest<HallResponse>;

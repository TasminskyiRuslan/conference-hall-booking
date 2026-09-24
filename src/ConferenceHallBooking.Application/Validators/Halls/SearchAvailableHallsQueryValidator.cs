using ConferenceHallBooking.Application.Features.Halls.Queries;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Halls;

/// <summary>
/// Validates SearchAvailableHallsQuery input.
/// </summary>
public class SearchAvailableHallsQueryValidator : AbstractValidator<SearchAvailableHallsQuery>
{
    public SearchAvailableHallsQueryValidator()
    {
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Requested capacity must be greater than zero.");

        RuleFor(x => x.StartTime)
            .Must(time => time >= DateTimeOffset.UtcNow)
            .WithMessage("Start time cannot be in the past.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be greater than start time.");
    }
}

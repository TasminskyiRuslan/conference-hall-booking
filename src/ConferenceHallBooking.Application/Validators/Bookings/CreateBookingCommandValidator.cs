using ConferenceHallBooking.Application.Features.Bookings.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Bookings;

/// <summary>
/// Validates CreateBookingCommand input.
/// </summary>
public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    /// <summary>Defines validation rules for booking creation.</summary>
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.HallId)
            .NotEmpty().WithMessage("Hall ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.StartTime)
            .Must(time => time >= DateTimeOffset.UtcNow)
            .WithMessage("Booking start time cannot be in the past.");

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("Booking duration must be greater than zero.")
            .LessThanOrEqualTo(24).WithMessage("Booking duration cannot exceed 24 hours.");

        RuleFor(x => x.OptionIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("Option IDs must not contain empty GUIDs.");

        RuleFor(x => x.OptionIds)
            .Must(ids => ids == null || ids.Count == ids.Distinct().Count())
            .WithMessage("Option IDs must not contain duplicates.");
    }
}

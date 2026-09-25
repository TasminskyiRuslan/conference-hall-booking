using ConferenceHallBooking.Application.Features.Halls.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Halls;

/// <summary>
/// Validates CreateHallCommand input.
/// </summary>
public class CreateHallCommandValidator : AbstractValidator<CreateHallCommand>
{
    /// <summary>Defines validation rules for hall creation.</summary>
    public CreateHallCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hall name is required.")
            .MaximumLength(100).WithMessage("Hall name must not exceed 100 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.");

        RuleFor(x => x.BaseHourlyRate)
            .GreaterThan(0).WithMessage("Base hourly rate must be greater than zero.");

        RuleFor(x => x.OptionIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("Option IDs must not contain empty GUIDs.");

        RuleFor(x => x.OptionIds)
            .Must(ids => ids == null || ids.Count == ids.Distinct().Count())
            .WithMessage("Option IDs must not contain duplicates.");
    }
}

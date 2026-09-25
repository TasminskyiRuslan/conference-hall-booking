using ConferenceHallBooking.Application.Features.Auth.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Auth;

/// <summary>
/// Validates LoginCommand input.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

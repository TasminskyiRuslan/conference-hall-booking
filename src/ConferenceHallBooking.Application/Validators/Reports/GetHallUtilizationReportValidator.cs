using ConferenceHallBooking.Application.Features.Reports.Queries;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Reports;

/// <summary>
/// Validates <see cref="GetHallUtilizationReportQuery"/> input.
/// </summary>
public class GetHallUtilizationReportValidator : AbstractValidator<GetHallUtilizationReportQuery>
{
    /// <summary>Defines the report date-range rules.</summary>
    public GetHallUtilizationReportValidator()
    {
        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.From)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Start date cannot be in the future.");
    }
}

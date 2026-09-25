using ConferenceHallBooking.Application.Features.Reports.Queries;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Reports;

/// <summary>
/// Validates <see cref="GetRevenueReportQuery"/> input.
/// </summary>
public class GetRevenueReportValidator : AbstractValidator<GetRevenueReportQuery>
{
    /// <summary>Defines the report date-range rules.</summary>
    public GetRevenueReportValidator()
    {
        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.From)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Start date cannot be in the future.");
    }
}

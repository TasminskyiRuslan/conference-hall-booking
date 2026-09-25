using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Application.Validators.Reports;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ConferenceHallBooking.UnitTests.Application.Features.Reports;

public class ReportValidatorTests
{
    private readonly GetRevenueReportValidator _revenueValidator = new();
    private readonly GetHallUtilizationReportValidator _utilizationValidator = new();
    private readonly GetBookingSummaryReportValidator _summaryValidator = new();

    [Fact]
    public async Task Validate_RevenueReport_WhenValid_ShouldNotHaveErrors()
    {
        var query = new GetRevenueReportQuery(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));

        var result = await _revenueValidator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_RevenueReport_WhenEndBeforeStart_ShouldHaveError()
    {
        var query = new GetRevenueReportQuery(
            new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var result = await _revenueValidator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public async Task Validate_UtilizationReport_WhenValid_ShouldNotHaveErrors()
    {
        var query = new GetHallUtilizationReportQuery(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero));

        var result = await _utilizationValidator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_UtilizationReport_WhenEndBeforeStart_ShouldHaveError()
    {
        var query = new GetHallUtilizationReportQuery(
            new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var result = await _utilizationValidator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public async Task Validate_SummaryReport_WhenValid_ShouldNotHaveErrors()
    {
        var query = new GetBookingSummaryReportQuery(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));

        var result = await _summaryValidator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_SummaryReport_WhenEndBeforeStart_ShouldHaveError()
    {
        var query = new GetBookingSummaryReportQuery(
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var result = await _summaryValidator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.To);
    }
}

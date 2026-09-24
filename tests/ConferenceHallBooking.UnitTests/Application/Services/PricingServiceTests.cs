using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Services;

public class PricingServiceTests
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);

    private static PricingService CreateService(params PricingRuleConfiguration[] rules)
    {
        var repository = Substitute.For<IPricingRuleRepository>();
        var domainRules = rules
            .Select(r => new PricingRule(r.StartTime, r.EndTime, r.Multiplier))
            .ToList();

        repository.GetOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(domainRules);

        return new PricingService(repository);
    }

    private sealed record PricingRuleConfiguration(TimeOnly StartTime, TimeOnly EndTime, decimal Multiplier);

    private static PricingRuleConfiguration Rule(int startHour, int startMinute, int endHour, int endMinute, decimal multiplier) =>
        new(new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute), multiplier);

    private static DateTimeOffset Date(int day, int hour, int minute = 0) =>
        new(2024, 1, day, hour, minute, 0, Offset);

    [Fact]
    public async Task CalculatePrice_WhenNoRules_ShouldUseBaseRate()
    {
        var service = CreateService();

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 10), Date(1, 13));

        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(300m);
    }

    [Fact]
    public async Task CalculatePrice_WhenNoRules_ShouldRoundHallCost()
    {
        var service = CreateService();

        var result = await service.CalculatePriceAsync(33.33m, null, Date(1, 8), Date(1, 10));

        result.HallCost.Should().Be(66.66m);
        result.TotalCost.Should().Be(66.66m);
    }

    [Fact]
    public async Task CalculatePrice_WhenRuleApplies_ShouldApplyMultiplier()
    {
        var service = CreateService(Rule(10, 0, 18, 0, 1.5m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 10), Date(1, 12));

        result.HallCost.Should().Be(300m);
    }

    [Fact]
    public async Task CalculatePrice_WhenMultipleRulesApply_ShouldSplitIntoSegments()
    {
        var service = CreateService(
            Rule(8, 0, 10, 0, 1.0m),
            Rule(10, 0, 18, 0, 1.5m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 9), Date(1, 11));

        result.HallCost.Should().Be(250m);
    }

    [Fact]
    public async Task CalculatePrice_WhenRuleGapExists_ShouldUseDefaultMultiplier()
    {
        var service = CreateService(Rule(10, 0, 14, 0, 2.0m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 9), Date(1, 17));

        result.HallCost.Should().Be(1200m);
    }

    [Fact]
    public async Task CalculatePrice_WithOptions_ShouldAddOptionsCost()
    {
        var service = CreateService();

        var result = await service.CalculatePriceAsync(100m, [50m], Date(1, 10), Date(1, 13));

        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(50m);
        result.TotalCost.Should().Be(350m);
    }

    [Fact]
    public async Task CalculatePrice_WithMultipleOptions_ShouldSumOptionsCost()
    {
        var service = CreateService();

        var result = await service.CalculatePriceAsync(100m, [50m, 30m, 20m], Date(1, 10), Date(1, 13));

        result.OptionsCost.Should().Be(100m);
        result.TotalCost.Should().Be(400m);
    }

    [Fact]
    public async Task CalculatePrice_WithEmptyOptionsCollection_ShouldUseZeroOptionsCost()
    {
        var service = CreateService();

        var result = await service.CalculatePriceAsync(100m, [], Date(1, 10), Date(1, 13));

        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(300m);
    }

    [Fact]
    public async Task CalculatePrice_WhenEndTimeBeforeStartTime_ShouldThrowInvalidBookingTimeException()
    {
        var service = CreateService();

        var act = async () => await service.CalculatePriceAsync(100m, null, Date(1, 13), Date(1, 10));

        await act.Should().ThrowAsync<InvalidBookingTimeException>();
    }

    [Fact]
    public async Task CalculatePrice_WhenEndTimeEqualsStartTime_ShouldThrowInvalidBookingTimeException()
    {
        var service = CreateService();
        var time = Date(1, 10);

        var act = async () => await service.CalculatePriceAsync(100m, null, time, time);

        await act.Should().ThrowAsync<InvalidBookingTimeException>();
    }

    [Fact]
    public async Task CalculatePrice_WhenBaseHourlyRateIsZero_ShouldThrowInvalidBaseHourlyRateException()
    {
        var service = CreateService();

        var act = async () => await service.CalculatePriceAsync(0m, null, Date(1, 10), Date(1, 13));

        await act.Should().ThrowAsync<InvalidBaseHourlyRateException>();
    }

    [Fact]
    public async Task CalculatePrice_WhenBaseHourlyRateIsNegative_ShouldThrowInvalidBaseHourlyRateException()
    {
        var service = CreateService();

        var act = async () => await service.CalculatePriceAsync(-50m, null, Date(1, 10), Date(1, 13));

        await act.Should().ThrowAsync<InvalidBaseHourlyRateException>();
    }

    [Fact]
    public async Task CalculatePrice_WhenBookingDuringNightHoursWithoutRule_ShouldUseBaseRate()
    {
        var service = CreateDefaultAppRules();

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 0), Date(1, 4));

        result.HallCost.Should().Be(400m);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(400m);
    }

    [Fact]
    public async Task CalculatePrice_WhenRulesOverlap_ShouldApplyFirstMatchingRule()
    {
        var service = CreateService(
            Rule(10, 0, 14, 0, 1.5m),
            Rule(12, 0, 13, 0, 2.0m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 12), Date(1, 13));

        // Non-overlapping rules are enforced at seed time; if data still overlaps,
        // the earliest StartTime wins because rules are ordered by StartTime.
        result.HallCost.Should().Be(150m);
    }

    [Fact]
    public async Task CalculatePrice_WhenBookingSpansMultipleDays_ShouldHandleCorrectly()
    {
        var service = CreateService(Rule(8, 0, 18, 0, 1.5m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 22), Date(3, 9));

        result.HallCost.Should().Be(4050m);
    }

    [Fact]
    public async Task CalculatePrice_WhenBookingCrossesMidnightAtRuleEnd_ShouldTreatRuleEndAsOutside()
    {
        var service = CreateService(Rule(18, 0, 23, 0, 0.8m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 22), Date(2, 0));

        result.HallCost.Should().Be(180m);
    }

    [Fact]
    public async Task CalculatePrice_WhenBookingCrossesMidnightAtRuleStart_ShouldApplyRuleFromStart()
    {
        var service = CreateService(Rule(0, 0, 6, 0, 0.5m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 23, 30), Date(2, 0, 30));

        result.HallCost.Should().Be(75m);
    }

    [Fact]
    public async Task CalculatePrice_WhenRuleEndsAt23AndBookingStartsAt23_ShouldUseBaseRate()
    {
        var service = CreateService(Rule(18, 0, 23, 0, 0.8m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 23), Date(2, 1));

        result.HallCost.Should().Be(200m);
    }

    [Fact]
    public async Task CalculatePrice_DefaultRules_MorningToNoonWithoutPeak_ShouldUseBaseRate()
    {
        var service = CreateDefaultAppRules();

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 9), Date(1, 12));

        result.HallCost.Should().Be(300m);
    }

    [Fact]
    public async Task CalculatePrice_DefaultRules_AfternoonWithoutPeak_ShouldUseBaseRate()
    {
        var service = CreateDefaultAppRules();

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 14), Date(1, 18));

        result.HallCost.Should().Be(400m);
    }

    [Fact]
    public async Task CalculatePrice_DefaultRules_PeakWindow_ShouldApply115Percent()
    {
        var service = CreateDefaultAppRules();

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 12), Date(1, 14));

        result.HallCost.Should().Be(230m);
    }

    [Fact]
    public async Task CalculatePrice_DefaultRules_EveningWindow_ShouldApply80Percent()
    {
        var service = CreateDefaultAppRules();

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 18), Date(1, 23));

        result.HallCost.Should().Be(400m);
    }

    [Fact]
    public async Task CalculatePrice_DefaultRules_MorningWindow_ShouldApply90Percent()
    {
        var service = CreateDefaultAppRules();

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 6), Date(1, 9));

        result.HallCost.Should().Be(270m);
    }

    private static PricingService CreateDefaultAppRules() => CreateService(
        Rule(6, 0, 9, 0, 0.90m),
        Rule(12, 0, 14, 0, 1.15m),
        Rule(18, 0, 23, 0, 0.80m));
}

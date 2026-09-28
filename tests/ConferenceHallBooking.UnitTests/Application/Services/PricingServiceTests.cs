using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Json;

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

        return new PricingService(
            repository,
            Options.Create(new PricingSettings { TimeZoneId = "Europe/Kyiv" }));
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
    public async Task CalculatePrice_WhenRulesOverlap_ShouldApplyFirstMatchingRule()
    {
        var service = CreateService(
            Rule(10, 0, 14, 0, 1.5m),
            Rule(12, 0, 13, 0, 2.0m));

        var result = await service.CalculatePriceAsync(100m, null, Date(1, 12), Date(1, 13));

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
    public async Task CalculatePrice_WithSeedRules_ShouldMatchTariffBoundaries()
    {
        var service = CreateService(
            Rule(6, 0, 9, 0, 0.9m),
            Rule(12, 0, 14, 0, 1.15m),
            Rule(18, 0, 23, 0, 0.8m));

        (await service.CalculatePriceAsync(1000m, null, Date(1, 6), Date(1, 9))).HallCost.Should().Be(2700m);
        (await service.CalculatePriceAsync(1000m, null, Date(1, 9), Date(1, 12))).HallCost.Should().Be(3000m);
        (await service.CalculatePriceAsync(1000m, null, Date(1, 12), Date(1, 14))).HallCost.Should().Be(2300m);
        (await service.CalculatePriceAsync(1000m, null, Date(1, 14), Date(1, 18))).HallCost.Should().Be(4000m);
        (await service.CalculatePriceAsync(1000m, null, Date(1, 18), Date(1, 23))).HallCost.Should().Be(4000m);
        (await service.CalculatePriceAsync(1000m, null, Date(1, 23), Date(2, 0))).HallCost.Should().Be(1000m);
    }

    [Fact]
    public async Task CalculatePrice_WhenSegmentSpansRuleBoundary_ShouldSplitAndApplyBothMultipliers()
    {
        var service = CreateService(
            Rule(6, 0, 9, 0, 0.9m),
            Rule(12, 0, 14, 0, 1.15m),
            Rule(18, 0, 23, 0, 0.8m));

        var result = await service.CalculatePriceAsync(1000m, null, Date(1, 11, 30), Date(1, 14, 30));

        result.HallCost.Should().Be(3300m);
    }

    [Fact]
    public async Task CalculatePrice_SameInstantInDifferentOffsets_ShouldProduceSamePrice()
    {
        var service = CreateService(Rule(18, 0, 23, 0, 0.8m));

        var utcStart = new DateTimeOffset(2024, 7, 15, 15, 0, 0, TimeSpan.Zero);
        var kyivStart = new DateTimeOffset(2024, 7, 15, 18, 0, 0, TimeSpan.FromHours(3));
        var westStart = new DateTimeOffset(2024, 7, 15, 11, 0, 0, TimeSpan.FromHours(-4));

        var utcPrice = await service.CalculatePriceAsync(1000m, null, utcStart, utcStart.AddHours(5));
        var kyivPrice = await service.CalculatePriceAsync(1000m, null, kyivStart, kyivStart.AddHours(5));
        var westPrice = await service.CalculatePriceAsync(1000m, null, westStart, westStart.AddHours(5));

        utcPrice.TotalCost.Should().Be(4000m);
        kyivPrice.TotalCost.Should().Be(4000m);
        westPrice.TotalCost.Should().Be(4000m);
    }

    [Fact]
    public void AppSettings_ShouldDeclarePricingRulesAndBusinessTimeZone()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ConferenceHallBooking.slnx")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the repository root must be reachable from the test output");
        var appsettingsPath = Path.Combine(directory!.FullName, "src", "ConferenceHallBooking.Api", "appsettings.json");
        using var document = JsonDocument.Parse(File.ReadAllText(appsettingsPath));

        var pricing = document.RootElement.GetProperty("PricingSettings");
        pricing.GetProperty("TimeZoneId").GetString().Should().Be("Europe/Kyiv");

        var rules = pricing.GetProperty("Rules");
        rules.GetArrayLength().Should().Be(3);
        rules[0].GetProperty("StartTime").GetString().Should().Be("06:00:00");
        rules[0].GetProperty("EndTime").GetString().Should().Be("09:00:00");
        rules[0].GetProperty("Multiplier").GetDecimal().Should().Be(0.9m);
        rules[1].GetProperty("StartTime").GetString().Should().Be("12:00:00");
        rules[1].GetProperty("EndTime").GetString().Should().Be("14:00:00");
        rules[1].GetProperty("Multiplier").GetDecimal().Should().Be(1.15m);
        rules[2].GetProperty("StartTime").GetString().Should().Be("18:00:00");
        rules[2].GetProperty("EndTime").GetString().Should().Be("23:00:00");
        rules[2].GetProperty("Multiplier").GetDecimal().Should().Be(0.8m);
    }
}

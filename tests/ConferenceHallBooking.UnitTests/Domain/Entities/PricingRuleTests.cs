using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Domain.Entities;

public class PricingRuleTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var rule = new PricingRule(new TimeOnly(8, 0), new TimeOnly(18, 0), 1.5m, 1);

        rule.Id.Should().NotBe(Guid.Empty);
        rule.StartTime.Should().Be(new TimeOnly(8, 0));
        rule.EndTime.Should().Be(new TimeOnly(18, 0));
        rule.Multiplier.Should().Be(1.5m);
        rule.SortOrder.Should().Be(1);
    }

    [Fact]
    public void Constructor_WhenEndTimeBeforeStartTime_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new PricingRule(new TimeOnly(18, 0), new TimeOnly(8, 0), 1.5m, 1);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(PricingRule.EndTime));
    }

    [Fact]
    public void Constructor_WhenEndTimeEqualsStartTime_ShouldThrowInvalidEntityFieldException()
    {
        var time = new TimeOnly(8, 0);
        var act = () => new PricingRule(time, time, 1.5m, 1);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(PricingRule.EndTime));
    }

    [Fact]
    public void Constructor_WhenMultiplierIsZero_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new PricingRule(new TimeOnly(8, 0), new TimeOnly(18, 0), 0m, 1);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(PricingRule.Multiplier));
    }

    [Fact]
    public void Constructor_WhenMultiplierIsNegative_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new PricingRule(new TimeOnly(8, 0), new TimeOnly(18, 0), -1m, 1);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(PricingRule.Multiplier));
    }
}

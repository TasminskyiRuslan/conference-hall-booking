using ConferenceHallBooking.Application.Features.Reports;
using ConferenceHallBooking.Domain.Entities;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Features.Reports;

public class ReportCalculationsTests
{
    private static readonly DateTimeOffset PeriodFrom = new(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset PeriodTo = new(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ProportionalRevenue_WithBookingFullyInsidePeriod_ShouldReturnFullPrice()
    {
        var booking = NewBooking(
            new DateTimeOffset(2026, 1, 12, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 14, 0, 0, 0, TimeSpan.Zero),
            4000m);

        var result = ReportCalculations.ProportionalRevenue(booking, PeriodFrom, PeriodTo);

        result.Should().Be(4000m);
    }

    [Fact]
    public void ProportionalRevenue_WithBookingCrossingEndBoundary_ShouldCountOnlyOverlap()
    {
        var booking = NewBooking(
            new DateTimeOffset(2026, 1, 18, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 22, 0, 0, 0, TimeSpan.Zero),
            4000m);

        var result = ReportCalculations.ProportionalRevenue(booking, PeriodFrom, PeriodTo);

        result.Should().Be(2000m);
    }

    [Fact]
    public void ProportionalRevenue_WithBookingCrossingStartBoundary_ShouldCountOnlyOverlap()
    {
        var booking = NewBooking(
            new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 12, 0, 0, 0, TimeSpan.Zero),
            4000m);

        var result = ReportCalculations.ProportionalRevenue(booking, PeriodFrom, PeriodTo);

        result.Should().Be(2000m);
    }

    [Fact]
    public void ProportionalRevenue_WithBookingOutsidePeriod_ShouldReturnZero()
    {
        var booking = NewBooking(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero),
            4000m);

        var result = ReportCalculations.ProportionalRevenue(booking, PeriodFrom, PeriodTo);

        result.Should().Be(0m);
    }

    [Fact]
    public void ProportionalRevenue_WithBookingEndingAtPeriodStart_ShouldReturnZero()
    {
        var booking = NewBooking(
            new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero),
            4000m);

        var result = ReportCalculations.ProportionalRevenue(booking, PeriodFrom, PeriodTo);

        result.Should().Be(0m);
    }

    private static Booking NewBooking(DateTimeOffset start, DateTimeOffset end, decimal price)
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var user = new User("tester@example.com", "fake-hash", "Tester");
        return new Booking(hall, user, start, end, price, price);
    }
}

using ConferenceHallBooking.Application.DTOs.Reports;
using ConferenceHallBooking.Application.Features.Reports.Handlers;
using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Reports;

public class ReportHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();

    private readonly GetRevenueReportHandler _revenueHandler;
    private readonly GetBookingSummaryReportHandler _summaryHandler;

    public ReportHandlerTests()
    {
        _revenueHandler = new GetRevenueReportHandler(_bookingRepository);
        _summaryHandler = new GetBookingSummaryReportHandler(_bookingRepository);
    }

    #region Revenue Report

    [Fact]
    public async Task GetRevenueReportHandler_WithBookings_ShouldReturnCorrectRevenue()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>
        {
            NewBooking(hall, from.AddDays(1), from.AddDays(1).AddHours(2), 2000m),
            NewBooking(hall, from.AddDays(5), from.AddDays(5).AddHours(3), 3000m)
        };
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _revenueHandler.Handle(
            new GetRevenueReportQuery(from, to), CancellationToken.None);

        report.TotalRevenue.Should().Be(5000m);
        report.ByHall.Should().HaveCount(1);
        report.ByHall.First().BookingCount.Should().Be(2);
    }

    [Fact]
    public async Task GetRevenueReportHandler_WithNoBookings_ShouldReturnZeroRevenue()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(new List<Booking>());

        var report = await _revenueHandler.Handle(
            new GetRevenueReportQuery(from, to), CancellationToken.None);

        report.TotalRevenue.Should().Be(0);
        report.ByHall.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRevenueReportHandler_ShouldPassPeriodToRepository()
    {
        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(new List<Booking>());

        await _revenueHandler.Handle(new GetRevenueReportQuery(from, to), CancellationToken.None);

        await _bookingRepository.Received(1).GetByDateRangeAsync(
            from, to, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRevenueReportHandler_WithBookingCrossingEndBoundary_ShouldCountRevenueProportionally()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);

        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns([NewBooking(hall,
                new DateTimeOffset(2026, 1, 18, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 22, 0, 0, 0, TimeSpan.Zero), 4000m)]);

        var report = await _revenueHandler.Handle(
            new GetRevenueReportQuery(from, to), CancellationToken.None);

        report.TotalRevenue.Should().Be(2000m);
        report.ByHall.First().BookingCount.Should().Be(1);
    }

    [Fact]
    public async Task GetRevenueReportHandler_WithBookingCrossingStartBoundary_ShouldCountRevenueProportionally()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);

        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns([NewBooking(hall,
                new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 12, 0, 0, 0, TimeSpan.Zero), 4000m)]);

        var report = await _revenueHandler.Handle(
            new GetRevenueReportQuery(from, to), CancellationToken.None);

        report.TotalRevenue.Should().Be(2000m);
    }

    #endregion

    #region Booking Summary Report

    [Fact]
    public async Task GetBookingSummaryReportHandler_WithBookings_ShouldReturnCorrectSummary()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>
        {
            NewBooking(hall, from.AddDays(1).AddHours(10), from.AddDays(1).AddHours(13), 3000m),
            NewBooking(hall, from.AddDays(5).AddHours(10), from.AddDays(5).AddHours(12), 2000m)
        };
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.TotalBookings.Should().Be(2);
        report.TotalRevenue.Should().Be(5000m);
        report.AverageBookingDurationHours.Should().Be(2.5m);
        report.AverageBookingRevenue.Should().Be(2500m);
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_WithNoBookings_ShouldReturnZeros()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(new List<Booking>());

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.TotalBookings.Should().Be(0);
        report.TotalRevenue.Should().Be(0);
        report.AverageBookingDurationHours.Should().Be(0);
        report.AverageBookingRevenue.Should().Be(0);
        report.PopularTimeSlots.Should().BeEmpty();
        report.PopularOptions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_ShouldReturnPopularTimeSlots()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>
        {
            NewBooking(hall, from.AddDays(1).AddHours(10), from.AddDays(1).AddHours(11), 1000m),
            NewBooking(hall, from.AddDays(2).AddHours(10), from.AddDays(2).AddHours(11), 1000m),
            NewBooking(hall, from.AddDays(3).AddHours(10), from.AddDays(3).AddHours(11), 1000m),
            NewBooking(hall, from.AddDays(4).AddHours(14), from.AddDays(4).AddHours(15), 1000m)
        };
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.PopularTimeSlots.First().Hour.Should().Be(10);
        report.PopularTimeSlots.First().BookingCount.Should().Be(3);
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_ShouldReturnPopularOptions()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var projector = new Option("Projector", 50m);
        var user = NewUser();
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>
        {
            new(hall, user, from.AddDays(1), from.AddDays(1).AddHours(2), 100m, 150m,
                [new BookingOption(projector, 50m)]),
            new(hall, user, from.AddDays(2), from.AddDays(2).AddHours(2), 100m, 150m,
                [new BookingOption(projector, 50m)])
        };
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.PopularOptions.Should().HaveCount(1);
        report.PopularOptions.First().OptionId.Should().Be(projector.Id);
        report.PopularOptions.First().Name.Should().Be("Projector");
        report.PopularOptions.First().BookingCount.Should().Be(2);
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_ShouldReturnAtMostTenPopularTimeSlots()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>();
        for (var hour = 0; hour < 11; hour++)
        {
            bookings.Add(NewBooking(
                hall, from.AddDays(1).AddHours(hour), from.AddDays(1).AddHours(hour + 1), 1000m));
        }
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.PopularTimeSlots.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_TimeSlotsWithEqualCounts_ShouldOrderByHour()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>();
        foreach (var hour in new[] { 20, 8, 14 })
        {
            bookings.Add(NewBooking(
                hall, from.AddDays(1).AddHours(hour), from.AddDays(1).AddHours(hour + 1), 1000m));
            bookings.Add(NewBooking(
                hall, from.AddDays(2).AddHours(hour), from.AddDays(2).AddHours(hour + 1), 1000m));
        }
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.PopularTimeSlots.Select(s => s.Hour).Should().Equal(8, 14, 20);
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_OptionsWithEqualCounts_ShouldOrderByName()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var zeta = new Option("Zeta", 50m);
        var alpha = new Option("Alpha", 50m);
        var mid = new Option("Mid", 50m);
        var user = NewUser();
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>
        {
            new(hall, user, from.AddDays(1), from.AddDays(1).AddHours(2), 100m, 250m,
                [
                    new BookingOption(zeta, 50m),
                    new BookingOption(alpha, 50m),
                    new BookingOption(mid, 50m)
                ])
        };
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.PopularOptions.Select(o => o.Name).Should().Equal("Alpha", "Mid", "Zeta");
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_MidpointDuration_ShouldRoundAwayFromZero()
    {
        var hall = new Hall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var bookings = new List<Booking>
        {
            NewBooking(hall, from.AddDays(1), from.AddDays(1).AddMinutes(6), 1000m),
            NewBooking(hall, from.AddDays(2), from.AddDays(2).AddMinutes(9), 1000m)
        };
        _bookingRepository.GetByDateRangeAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(bookings);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.AverageBookingDurationHours.Should().Be(0.13m);
    }

    #endregion

    private static User NewUser() => new("tester@example.com", "fake-hash", "Tester");

    private static Booking NewBooking(Hall hall, DateTimeOffset start, DateTimeOffset end, decimal price)
    {
        return new Booking(hall, NewUser(), start, end, price, price);
    }
}

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

    public ReportHandlerTests()
    {
        _revenueHandler = new GetRevenueReportHandler(_bookingRepository);
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

    private static Booking NewBooking(Hall hall, DateTimeOffset start, DateTimeOffset end, decimal price)
    {
        var user = new User("tester@example.com", "fake-hash", "Tester");
        return new Booking(hall, user, start, end, price, price);
    }
}

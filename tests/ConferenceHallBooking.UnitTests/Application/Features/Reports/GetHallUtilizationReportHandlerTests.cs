using ConferenceHallBooking.Application.Features.Reports.Handlers;
using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.UnitTests.Application.Features.Reports;

/// <summary>
/// Uses a real InMemory database: Hall.Bookings is populated only by
/// EF navigation fixup, so mocks cannot provide booking data.
/// </summary>
public class GetHallUtilizationReportHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetHallUtilizationReportHandler _utilizationHandler;

    public GetHallUtilizationReportHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _utilizationHandler = new GetHallUtilizationReportHandler(new HallRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetUtilizationReportHandler_WithBookings_ShouldCalculateUtilization()
    {
        var hall = SeedHall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hall, from.AddDays(1), from.AddDays(2), 1000m);

        var report = await _utilizationHandler.Handle(
            new GetHallUtilizationReportQuery(from, to), CancellationToken.None);

        report.Halls.Should().HaveCount(1);
        report.Halls.First().BookedHours.Should().Be(24);
        report.Halls.First().UtilizationPercent.Should().Be(14.3m);
    }

    [Fact]
    public async Task GetUtilizationReportHandler_WithNoHalls_ShouldReturnEmpty()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        var report = await _utilizationHandler.Handle(
            new GetHallUtilizationReportQuery(from, to), CancellationToken.None);

        report.Halls.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUtilizationReportHandler_WithBookingCrossingBoundary_ShouldClampToPeriod()
    {
        var hall = SeedHall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hall, new DateTimeOffset(2026, 1, 7, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 9, 0, 0, 0, TimeSpan.Zero), 1000m);

        var report = await _utilizationHandler.Handle(
            new GetHallUtilizationReportQuery(from, to), CancellationToken.None);

        report.Halls.First().BookedHours.Should().Be(12);
        report.Halls.First().UtilizationPercent.Should().Be(7.1m);
    }

    [Fact]
    public async Task GetUtilizationReportHandler_WithMultipleHalls_ShouldOrderDescending()
    {
        var hallA = SeedHall("Hall A", 50, 1000m);
        var hallB = SeedHall("Hall B", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hallA, from.AddDays(1), from.AddDays(2), 1000m);
        SeedBooking(hallB, from.AddDays(1), from.AddDays(4), 1000m);

        var report = await _utilizationHandler.Handle(
            new GetHallUtilizationReportQuery(from, to), CancellationToken.None);

        report.Halls.First().HallId.Should().Be(hallB.Id);
        report.Halls.Last().HallId.Should().Be(hallA.Id);
    }

    private Hall SeedHall(string name, int capacity, decimal rate)
    {
        var hall = new Hall(name, capacity, rate);
        _context.Halls.Add(hall);
        _context.SaveChanges();
        return hall;
    }

    private void SeedBooking(Hall hall, DateTimeOffset start, DateTimeOffset end, decimal price)
    {
        var user = new User("tester@example.com", "fake-hash", "Tester");
        _context.Bookings.Add(new Booking(hall, user, start, end, price, price));
        _context.SaveChanges();
    }
}

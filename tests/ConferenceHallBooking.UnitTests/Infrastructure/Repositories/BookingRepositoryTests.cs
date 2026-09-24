using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Repositories;

public class BookingRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = TestDbContextFactory.Create();
    private readonly BookingRepository _repository;

    public BookingRepositoryTests()
    {
        _repository = new BookingRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static Hall CreateHall(string name = "Hall A")
        => new(name, 50, 1000m);

    private static User CreateUser(string email = "owner@test.com")
        => new(email, "hash", "Test User");

    private static DateTimeOffset Day(int day, int hour)
        => new(2026, 1, day, hour, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnBookingWithHallAndOptions()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var option = new Option("Projector", 500m);
        var booking = new Booking(
            hall, user, Day(10, 10), Day(10, 12), 2000m, 2500m,
            [new BookingOption(option, 500m)]);
        _context.AddRange(hall, user, option, booking);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(booking.Id);

        result.Should().NotBeNull();
        result!.Hall.Id.Should().Be(hall.Id);
        result.BookingOptions.Should().ContainSingle();
        result.BookingOptions.First().Option.Id.Should().Be(option.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task HasOverlappingBookingAsync_WhenOverlap_ShouldReturnTrue()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var booking = new Booking(hall, user, Day(10, 10), Day(10, 12), 2000m, 2000m);
        _context.AddRange(hall, user, booking);
        await _context.SaveChangesAsync();

        var result = await _repository.HasOverlappingBookingAsync(
            hall.Id, Day(10, 11), Day(10, 13));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasOverlappingBookingAsync_WhenNoOverlap_ShouldReturnFalse()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var booking = new Booking(hall, user, Day(10, 10), Day(10, 12), 2000m, 2000m);
        _context.AddRange(hall, user, booking);
        await _context.SaveChangesAsync();

        var result = await _repository.HasOverlappingBookingAsync(
            hall.Id, Day(10, 13), Day(10, 14));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetBookingCountByHallIdAsync_ShouldCountOnlyThatHall()
    {
        var hallA = CreateHall("A");
        var hallB = CreateHall("B");
        var user = CreateUser();
        _context.AddRange(
            hallA, hallB, user,
            new Booking(hallA, user, Day(10, 10), Day(10, 11), 1000m, 1000m),
            new Booking(hallA, user, Day(11, 10), Day(11, 11), 1000m, 1000m),
            new Booking(hallB, user, Day(12, 10), Day(12, 11), 1000m, 1000m));
        await _context.SaveChangesAsync();

        var result = await _repository.GetBookingCountByHallIdAsync(hallA.Id);

        result.Should().Be(2);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnOnlyOverlappingBookings()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var inRange = new Booking(hall, user, Day(15, 10), Day(15, 12), 2000m, 2000m);
        var outOfRange = new Booking(hall, user, Day(20, 10), Day(20, 12), 2000m, 2000m);
        _context.AddRange(hall, user, inRange, outOfRange);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByDateRangeAsync(Day(15, 9), Day(15, 18));

        result.Should().ContainSingle(b => b.Id == inRange.Id);
        result.Should().NotContain(b => b.Id == outOfRange.Id);
        result[0].Hall.Id.Should().Be(hall.Id);
    }

    [Fact]
    public async Task GetBlockedOptionIdsForHallAsync_WhenEmptyCandidates_ShouldReturnEmpty()
    {
        var result = await _repository.GetBlockedOptionIdsForHallAsync(
            Guid.NewGuid(), []);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBlockedOptionIdsForHallAsync_WhenFutureBookingHasOption_ShouldReturnOptionId()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var option = new Option("Sound", 700m);
        var futureStart = DateTimeOffset.UtcNow.AddDays(1);
        var futureEnd = futureStart.AddHours(2);
        var booking = new Booking(
            hall, user, futureStart, futureEnd, 1000m, 1700m,
            [new BookingOption(option, 700m)]);
        _context.AddRange(hall, user, option, booking);
        await _context.SaveChangesAsync();

        var result = await _repository.GetBlockedOptionIdsForHallAsync(
            hall.Id, [option.Id]);

        result.Should().ContainSingle(id => id == option.Id);
    }

    [Fact]
    public async Task GetBlockedOptionIdsForHallAsync_WhenBookingEnded_ShouldNotReturnOption()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var option = new Option("Sound", 700m);
        var booking = new Booking(
            hall, user,
            DateTimeOffset.UtcNow.AddDays(-3),
            DateTimeOffset.UtcNow.AddDays(-2),
            1000m, 1700m,
            [new BookingOption(option, 700m)]);
        _context.AddRange(hall, user, option, booking);
        await _context.SaveChangesAsync();

        var result = await _repository.GetBlockedOptionIdsForHallAsync(
            hall.Id, [option.Id]);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ShouldPersist()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var booking = new Booking(hall, user, Day(10, 10), Day(10, 12), 2000m, 2000m);
        _context.AddRange(hall, user);
        await _context.SaveChangesAsync();

        await _repository.AddAsync(booking);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(booking.Id);
        result.Should().NotBeNull();
    }
}

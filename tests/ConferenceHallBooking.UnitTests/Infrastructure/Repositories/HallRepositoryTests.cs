using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Repositories;

public class HallRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = TestDbContextFactory.Create();
    private readonly HallRepository _repository;

    public HallRepositoryTests()
    {
        _repository = new HallRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static Hall CreateHall(string name = "Hall A", int capacity = 50, decimal rate = 1000m)
        => new(name, capacity, rate);

    private static User CreateUser(string email = "owner@test.com")
        => new(email, "hash", "Test User");

    private static DateTimeOffset Day(int day, int hour)
        => new(2026, 1, day, hour, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnHallWithOption()
    {
        var option = new Option("Projector", 500m);
        var hall = CreateHall();
        hall.AddOption(option);
        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(hall.Id);

        result.Should().NotBeNull();
        result!.HallOptions.Should().ContainSingle(ho => ho.Option.Id == option.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableHallsAsync_WhenHallBooked_ShouldExcludeIt()
    {
        var hall = CreateHall();
        var user = CreateUser();
        var booking = new Booking(hall, user, Day(10, 10), Day(10, 12), 2000m, 2000m);
        _context.AddRange(hall, user, booking);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAvailableHallsAsync(Day(10, 11), Day(10, 11), 1);

        result.Should().NotContain(h => h.Id == hall.Id);
    }

    [Fact]
    public async Task GetAvailableHallsAsync_WhenCapacityTooLow_ShouldExcludeHall()
    {
        var hall = CreateHall(capacity: 10);
        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAvailableHallsAsync(Day(10, 10), Day(10, 11), 20);

        result.Should().NotContain(h => h.Id == hall.Id);
    }

    [Fact]
    public async Task GetAvailableHallsAsync_WhenFree_ShouldIncludeHallWithOptions()
    {
        var option = new Option("Wi-Fi", 300m);
        var hall = CreateHall();
        hall.AddOption(option);
        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAvailableHallsAsync(Day(10, 10), Day(10, 11), 10);

        result.Should().ContainSingle(h => h.Id == hall.Id);
        result[0].HallOptions.Should().ContainSingle(ho => ho.Option.Id == option.Id);
    }

    [Fact]
    public async Task GetHallsWithBookingsInRangeAsync_ShouldReturnOnlyOverlappingBookingsPerHall()
    {
        var hall = CreateHall("Hall");
        var user = CreateUser();
        var inRange = new Booking(hall, user, Day(15, 10), Day(15, 12), 2000m, 2000m);
        var outOfRange = new Booking(hall, user, Day(20, 10), Day(20, 12), 2000m, 2000m);
        _context.AddRange(hall, user, inRange, outOfRange);
        await _context.SaveChangesAsync();

        var result = await _repository.GetHallsWithBookingsInRangeAsync(Day(15, 9), Day(15, 18));

        result.Should().ContainSingle(h => h.Id == hall.Id);
        result[0].Bookings.Should().ContainSingle(b => b.Id == inRange.Id);
        result[0].Bookings.Should().NotContain(b => b.Id == outOfRange.Id);
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenExists_ShouldReturnTrue()
    {
        _context.Halls.Add(CreateHall("Unique"));
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByNameAsync("Unique");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenMissing_ShouldReturnFalse()
    {
        var result = await _repository.ExistsByNameAsync("Missing");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ShouldPersist()
    {
        var hall = CreateHall("New Hall");
        await _repository.AddAsync(hall);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(hall.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Hall");
    }

    [Fact]
    public async Task Delete_ThenSave_ShouldRemoveHall()
    {
        var hall = CreateHall();
        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();

        _repository.Delete(hall);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(hall.Id);
        result.Should().BeNull();
    }
}

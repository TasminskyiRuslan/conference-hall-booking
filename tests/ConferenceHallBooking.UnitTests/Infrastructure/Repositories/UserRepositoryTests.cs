using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = TestDbContextFactory.Create();
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetByEmailAsync_WhenExists_ShouldReturnUser()
    {
        var user = new User("user@test.com", "hash", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("user@test.com");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenMissing_ShouldReturnNull()
    {
        var result = await _repository.GetByEmailAsync("missing@test.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenExists_ShouldReturnTrue()
    {
        _context.Users.Add(new User("user@test.com", "hash", "Test User"));
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByEmailAsync("user@test.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenMissing_ShouldReturnFalse()
    {
        var result = await _repository.ExistsByEmailAsync("missing@test.com");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ThenGetByEmail_ShouldPersist()
    {
        var user = new User("new@test.com", "hash", "New User");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("new@test.com");
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }
}

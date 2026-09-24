using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Data;

public class UnitOfWorkTests : IDisposable
{
    private readonly AppDbContext _context = TestDbContextFactory.Create();
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose() => _context.Dispose();

    private static Hall CreateHall(string name = "Hall A")
        => new(name, 50, 1000m);

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistPendingEntities()
    {
        var hall = CreateHall("Persisted");
        _context.Halls.Add(hall);

        var affected = await _unitOfWork.SaveChangesAsync();

        affected.Should().BeGreaterThan(0);
        var loaded = await _context.Halls.AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == hall.Id);
        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Persisted");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoChanges_ShouldReturnZero()
    {
        var affected = await _unitOfWork.SaveChangesAsync();

        affected.Should().Be(0);
    }

    [Fact]
    public void ExecuteInTransactionAsync_OnInMemoryProvider_ShouldThrowNotSupported()
    {
        // InMemory does not support transactions; covered by integration tests on PostgreSQL.
        var act = async () => await _unitOfWork.ExecuteInTransactionAsync(_ => Task.CompletedTask);

        act.Should().ThrowAsync<Exception>();
    }
}

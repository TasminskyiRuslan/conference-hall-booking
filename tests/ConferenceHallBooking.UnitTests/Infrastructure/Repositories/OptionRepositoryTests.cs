using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Repositories;

public class OptionRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = TestDbContextFactory.Create();
    private readonly OptionRepository _repository;

    public OptionRepositoryTests()
    {
        _repository = new OptionRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnOnlyRequestedIds()
    {
        var projector = new Option("Projector", 500m);
        var wifi = new Option("Wi-Fi", 300m);
        var sound = new Option("Sound", 700m);
        _context.AddRange(projector, wifi, sound);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdsAsync([projector.Id, sound.Id]);

        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == projector.Id);
        result.Should().Contain(o => o.Id == sound.Id);
        result.Should().NotContain(o => o.Id == wifi.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNoneMatch_ShouldReturnEmpty()
    {
        _context.Options.Add(new Option("Projector", 500m));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdsAsync([Guid.NewGuid()]);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_WhenEmptyIds_ShouldReturnEmpty()
    {
        var result = await _repository.GetByIdsAsync([]);

        result.Should().BeEmpty();
    }
}

using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Repositories;

public class PricingRuleRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = TestDbContextFactory.Create();
    private readonly PricingRuleRepository _repository;

    public PricingRuleRepositoryTests()
    {
        _repository = new PricingRuleRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetOrderedAsync_ShouldOrderByStartTimeAscending()
    {
        _context.PricingRules.AddRange(
            new PricingRule(new TimeOnly(18, 0), new TimeOnly(23, 0), 0.80m),
            new PricingRule(new TimeOnly(6, 0), new TimeOnly(9, 0), 0.90m),
            new PricingRule(new TimeOnly(12, 0), new TimeOnly(14, 0), 1.15m));
        await _context.SaveChangesAsync();

        var result = await _repository.GetOrderedAsync();

        result.Should().HaveCount(3);
        result[0].StartTime.Should().Be(new TimeOnly(6, 0));
        result[1].StartTime.Should().Be(new TimeOnly(12, 0));
        result[2].StartTime.Should().Be(new TimeOnly(18, 0));
    }

    [Fact]
    public async Task GetOrderedAsync_WhenEmpty_ShouldReturnEmpty()
    {
        var result = await _repository.GetOrderedAsync();

        result.Should().BeEmpty();
    }
}

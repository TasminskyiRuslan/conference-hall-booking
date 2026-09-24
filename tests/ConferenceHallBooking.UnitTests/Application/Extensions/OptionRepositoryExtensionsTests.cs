using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Extensions;

public class OptionRepositoryExtensionsTests
{
    private readonly IOptionRepository _repository = Substitute.For<IOptionRepository>();

    [Fact]
    public async Task GetByIdsOrThrow_WhenIdsEmpty_ShouldReturnEmptyWithoutDbCall()
    {
        var result = await _repository.GetByIdsOrThrowAsync([]);

        result.Should().BeEmpty();
        await _repository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdsOrThrow_WhenAllIdsExist_ShouldReturnOptions()
    {
        var option1 = new Option("Projector", 50m);
        var option2 = new Option("Wi-Fi", 30m);
        var ids = new[] { option1.Id, option2.Id };

        _repository.GetByIdsAsync(Arg.Is<IReadOnlyCollection<Guid>>(c => c.Count == 2), Arg.Any<CancellationToken>())
            .Returns(new List<Option> { option1, option2 });

        var result = await _repository.GetByIdsOrThrowAsync(ids);

        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == option1.Id);
        result.Should().Contain(o => o.Id == option2.Id);
    }

    [Fact]
    public async Task GetByIdsOrThrow_WhenSomeIdsMissing_ShouldThrowOptionsNotFoundException()
    {
        var existing = new Option("Projector", 50m);
        var missingId = Guid.NewGuid();
        var ids = new[] { existing.Id, missingId };

        _repository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Option> { existing });

        var act = () => _repository.GetByIdsOrThrowAsync(ids);

        var exception = await act.Should().ThrowAsync<OptionsNotFoundException>();
        exception.Which.MissingIds.Should().Contain(missingId);
        exception.Which.MissingIds.Should().NotContain(existing.Id);
    }

    [Fact]
    public async Task GetByIdsOrThrow_WhenDuplicateIds_ShouldQueryDistinctOnce()
    {
        var option = new Option("Projector", 50m);
        var ids = new[] { option.Id, option.Id };

        _repository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(c => c.Count == 1),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { option });

        var result = await _repository.GetByIdsOrThrowAsync(ids);

        result.Should().HaveCount(1);
        await _repository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(c => c.Count == 1),
            Arg.Any<CancellationToken>());
    }
}

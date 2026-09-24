using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Application.Validators.Halls;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Halls;

public class SearchAvailableHallsQueryValidatorTests
{
    private readonly SearchAvailableHallsQueryValidator _validator = new();

    [Fact]
    public async Task Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        var query = new SearchAvailableHallsQuery(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(3),
            10);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_WhenCapacityIsInvalid_ShouldHaveValidationError(int invalidCapacity)
    {
        var query = new SearchAvailableHallsQuery(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(3),
            invalidCapacity);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SearchAvailableHallsQuery.Capacity));
    }

    [Fact]
    public async Task Validate_WhenStartTimeIsInThePast_ShouldHaveValidationError()
    {
        var query = new SearchAvailableHallsQuery(
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow.AddHours(3),
            10);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SearchAvailableHallsQuery.StartTime));
    }

    [Fact]
    public async Task Validate_WhenEndTimeIsBeforeStartTime_ShouldHaveValidationError()
    {
        var startTime = DateTimeOffset.UtcNow.AddHours(3);
        var query = new SearchAvailableHallsQuery(
            startTime,
            startTime.AddHours(-1),
            10);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SearchAvailableHallsQuery.EndTime));
    }
}

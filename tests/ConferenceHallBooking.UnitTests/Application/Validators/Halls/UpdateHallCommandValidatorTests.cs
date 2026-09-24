using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Validators.Halls;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Halls;

public class UpdateHallCommandValidatorTests
{
    private readonly UpdateHallCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        var command = new UpdateHallCommand(Guid.NewGuid(), "Test Hall", 10, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenCommandWithValidOptionIds_ShouldNotHaveValidationError()
    {
        var command = new UpdateHallCommand(Guid.NewGuid(), "Test Hall", 10, 100m, [Guid.NewGuid(), Guid.NewGuid()]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var command = new UpdateHallCommand(Guid.NewGuid(), string.Empty, 10, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHallCommand.Name));
    }

    [Fact]
    public async Task Validate_WhenNameExceeds100Characters_ShouldHaveValidationError()
    {
        var longName = new string('a', 101);
        var command = new UpdateHallCommand(Guid.NewGuid(), longName, 10, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHallCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_WhenCapacityIsInvalid_ShouldHaveValidationError(int invalidCapacity)
    {
        var command = new UpdateHallCommand(Guid.NewGuid(), "Test Hall", invalidCapacity, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHallCommand.Capacity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50.5)]
    public async Task Validate_WhenBaseHourlyRateIsInvalid_ShouldHaveValidationError(decimal invalidRate)
    {
        var command = new UpdateHallCommand(Guid.NewGuid(), "Test Hall", 10, invalidRate, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHallCommand.BaseHourlyRate));
    }

    [Fact]
    public async Task Validate_WhenOptionIdsContainsEmptyGuid_ShouldHaveValidationError()
    {
        var command = new UpdateHallCommand(Guid.NewGuid(), "Test Hall", 10, 100m, [Guid.NewGuid(), Guid.Empty]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHallCommand.OptionIds));
    }

    [Fact]
    public async Task Validate_WhenOptionIdsContainsDuplicates_ShouldHaveValidationError()
    {
        var id = Guid.NewGuid();
        var command = new UpdateHallCommand(Guid.NewGuid(), "Test Hall", 10, 100m, [id, id]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHallCommand.OptionIds));
    }
}

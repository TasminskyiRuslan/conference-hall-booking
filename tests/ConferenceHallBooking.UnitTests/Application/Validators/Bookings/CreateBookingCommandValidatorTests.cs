using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Validators.Bookings;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Bookings;

public class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator = new();

    private static CreateBookingCommand NewCommand(
        Guid? hallId = null,
        Guid? userId = null,
        DateTimeOffset? startTime = null,
        decimal durationHours = 2m,
        List<Guid>? optionIds = null) =>
        new(
            hallId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            startTime ?? DateTimeOffset.UtcNow.AddDays(1),
            durationHours,
            optionIds);

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(-0.5)]
    public async Task Validate_WhenDurationIsInvalid_ShouldHaveValidationError(decimal invalidDuration)
    {
        var command = NewCommand(durationHours: invalidDuration);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.DurationHours));
    }

    [Theory]
    [InlineData(25)]
    [InlineData(48)]
    public async Task Validate_WhenDurationExceeds24Hours_ShouldHaveValidationError(decimal invalidDuration)
    {
        var command = NewCommand(durationHours: invalidDuration);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.DurationHours));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2.5)]
    [InlineData(8)]
    public async Task Validate_WhenDurationIsValid_ShouldNotHaveDurationError(decimal validDuration)
    {
        var command = NewCommand(durationHours: validDuration);

        var result = await _validator.ValidateAsync(command);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateBookingCommand.DurationHours));
    }

    [Fact]
    public async Task Validate_WhenHallIdIsEmpty_ShouldHaveValidationError()
    {
        var command = NewCommand(hallId: Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.HallId));
    }

    [Fact]
    public async Task Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        var command = NewCommand(userId: Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.UserId));
    }

    [Fact]
    public async Task Validate_WhenStartTimeIsInThePast_ShouldHaveValidationError()
    {
        var command = NewCommand(startTime: DateTimeOffset.UtcNow.AddDays(-1));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.StartTime));
    }

    [Fact]
    public async Task Validate_WhenOptionIdsContainEmptyGuid_ShouldHaveValidationError()
    {
        var command = NewCommand(optionIds: [Guid.NewGuid(), Guid.Empty]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.OptionIds));
    }

    [Fact]
    public async Task Validate_WhenOptionIdsContainDuplicates_ShouldHaveValidationError()
    {
        var optionId = Guid.NewGuid();
        var command = NewCommand(optionIds: [optionId, optionId]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.OptionIds));
    }

    [Fact]
    public async Task Validate_WhenEverythingIsValid_ShouldBeValid()
    {
        var command = NewCommand(optionIds: [Guid.NewGuid(), Guid.NewGuid()]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

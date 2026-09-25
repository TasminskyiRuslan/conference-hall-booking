using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Validators.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenValid_ShouldNotHaveErrors()
    {
        var command = new RegisterCommand("user@email.com", "password123", "password123", "John Doe");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_WhenEmailIsEmpty_ShouldHaveError(string email)
    {
        var command = new RegisterCommand(email, "password123", "password123", "John Doe");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ShouldHaveError()
    {
        var command = new RegisterCommand("not-an-email", "password123", "password123", "John Doe");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WhenEmailExceeds200Characters_ShouldHaveError()
    {
        var email = new string('a', 199) + "@example.com";
        var command = new RegisterCommand(email, "password123", "password123", "John Doe");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public async Task Validate_WhenPasswordIsTooShort_ShouldHaveError(string password)
    {
        var command = new RegisterCommand("user@email.com", password, password, "John Doe");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WhenFullNameIsEmpty_ShouldHaveError()
    {
        var command = new RegisterCommand("user@email.com", "password123", "password123", "");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public async Task Validate_WhenPasswordsDoNotMatch_ShouldHaveError()
    {
        var command = new RegisterCommand("user@email.com", "password123", "different123", "John Doe");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public async Task Validate_WhenConfirmPasswordIsEmpty_ShouldHaveError()
    {
        var command = new RegisterCommand("user@email.com", "password123", "", "John Doe");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}

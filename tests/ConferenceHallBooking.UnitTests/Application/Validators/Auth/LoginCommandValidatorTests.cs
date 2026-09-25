using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Validators.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenValid_ShouldNotHaveErrors()
    {
        var command = new LoginCommand("user@email.com", "password123");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_WhenEmailIsEmpty_ShouldHaveError(string email)
    {
        var command = new LoginCommand(email, "password123");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ShouldHaveError()
    {
        var command = new LoginCommand("not-an-email", "password123");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WhenEmailExceeds200Characters_ShouldHaveError()
    {
        var email = new string('a', 199) + "@example.com";
        var command = new LoginCommand(email, "password123");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_WhenPasswordIsEmpty_ShouldHaveError(string password)
    {
        var command = new LoginCommand("user@email.com", password);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

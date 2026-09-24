using ConferenceHallBooking.Application.Behaviours;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using ValidationException = ConferenceHallBooking.Domain.Exceptions.ValidationException;

namespace ConferenceHallBooking.UnitTests.Application.Behaviours;

public class ValidationBehaviorTests
{
    private readonly IValidator<TestRequest> _validator = Substitute.For<IValidator<TestRequest>>();
    private readonly RequestHandlerDelegate<TestResponse> _next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>([]);
        var request = new TestRequest("test");

        _ = await behavior.Handle(request, _next, CancellationToken.None);

        await _next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_ShouldCallNext()
    {
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([_validator]);
        var request = new TestRequest("valid");

        _ = await behavior.Handle(request, _next, CancellationToken.None);

        await _next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldThrowValidationException()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required.")
        };
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([_validator]);
        var request = new TestRequest("");

        var act = () => behavior.Handle(request, _next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldGroupErrorsByProperty()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required."),
            new("Name", "Name must not exceed 100 characters."),
            new("Capacity", "Capacity must be greater than zero.")
        };
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([_validator]);
        var request = new TestRequest("");

        var act = () => behavior.Handle(request, _next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
        exception.Which.Errors.Should().ContainKey("Name");
        exception.Which.Errors.Should().ContainKey("Capacity");
        exception.Which.Errors["Name"].Should().HaveCount(2);
        exception.Which.Errors["Capacity"].Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenMultipleValidators_ShouldRunAllValidators()
    {
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        var validator2 = Substitute.For<IValidator<TestRequest>>();

        validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([validator1, validator2]);
        var request = new TestRequest("valid");

        await behavior.Handle(request, _next, CancellationToken.None);

        await validator1.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>());
        await validator2.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>());
        await _next.Received(1)();
    }

    public record TestRequest(string Name) : IRequest<TestResponse>;

    public record TestResponse(string Value);
}

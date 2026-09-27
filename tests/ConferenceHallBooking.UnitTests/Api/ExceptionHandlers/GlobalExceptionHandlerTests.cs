using ConferenceHallBooking.Api.ExceptionHandlers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Api.ExceptionHandlers;

public class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _sut = new GlobalExceptionHandler(_logger);
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldReturn404()
    {
        var context = CreateHttpContext();
        var exception = new NotFoundException<Hall>(Guid.NewGuid());

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturn400()
    {
        var context = CreateHttpContext();
        var errors = new Dictionary<string, string[]> { ["Name"] = ["Required"] };
        var exception = new ValidationException(errors);

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithHallAlreadyBookedException_ShouldReturn409()
    {
        var context = CreateHttpContext();
        var exception = new HallAlreadyBookedException(
            Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task TryHandleAsync_WithHallHasBookingsException_ShouldReturn409()
    {
        var context = CreateHttpContext();
        var exception = new HallHasBookingsException(Guid.NewGuid(), 3);

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_ShouldReturn500()
    {
        var context = CreateHttpContext();
        var exception = new NotSupportedException("Something went wrong");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnProblemDetailsJson()
    {
        var context = CreateHttpContext();
        var exception = new NotFoundException<Hall>(Guid.NewGuid());

        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
        var body = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(body);

        problemDetails.Should().NotBeNull();
        problemDetails!.Type.Should().Contain("rfc7807");
        problemDetails.Status.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetProblemJsonContentType()
    {
        var context = CreateHttpContext();
        var exception = new NotFoundException<Hall>(Guid.NewGuid());

        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeTraceId()
    {
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-id-123";
        var exception = new NotFoundException<Hall>(Guid.NewGuid());

        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
        var body = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();

        body.Should().Contain("test-trace-id-123");
    }

    [Fact]
    public async Task TryHandleAsync_WithInvalidBookingTimeException_ShouldReturn400()
    {
        var context = CreateHttpContext();
        var exception = new InvalidBookingTimeException(
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(-1));

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithHallNameAlreadyExistsException_ShouldReturn409()
    {
        var context = CreateHttpContext();
        var exception = new HallNameAlreadyExistsException("Hall A");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task TryHandleAsync_WithOptionsNotFoundException_ShouldReturn404()
    {
        var context = CreateHttpContext();
        var exception = new OptionsNotFoundException(new List<Guid> { Guid.NewGuid() });

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_WithInvalidBaseHourlyRateException_ShouldReturn400()
    {
        var context = CreateHttpContext();
        var exception = new InvalidBaseHourlyRateException(-100m);

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithHallOptionNotSupportedException_ShouldReturn409()
    {
        var context = CreateHttpContext();
        var exception = new HallOptionNotSupportedException(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task TryHandleAsync_WithEmailAlreadyExistsException_ShouldReturn409()
    {
        var context = CreateHttpContext();
        var exception = new EmailAlreadyExistsException("test@test.com");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task TryHandleAsync_WithInvalidCredentialsException_ShouldReturn401()
    {
        var context = CreateHttpContext();
        var exception = new InvalidCredentialsException();

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task TryHandleAsync_WithInvalidEntityFieldException_ShouldReturn400()
    {
        var context = CreateHttpContext();
        var exception = new InvalidEntityFieldException("Hall", "Name", "cannot be empty");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithBookingAccessException_ShouldReturn403()
    {
        var context = CreateHttpContext();
        var exception = new BookingAccessException();

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task TryHandleAsync_WithBookingOverlapException_ShouldReturn409WithErrorCode()
    {
        var context = CreateHttpContext();
        var exception = new BookingOverlapException(new InvalidOperationException("race"));

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);

        context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
        var body = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("HALL_ALREADY_BOOKED");
    }

    [Fact]
    public async Task TryHandleAsync_WithUniqueConstraintViolationException_ShouldReturn409WithErrorCode()
    {
        var context = CreateHttpContext();
        var exception = new UniqueConstraintViolationException(new InvalidOperationException("race"));

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);

        context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
        var body = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("UNIQUE_CONSTRAINT_VIOLATION");
    }

    [Fact]
    public async Task TryHandleAsync_WithInvalidOperationException_ShouldReturn500()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Cannot modify a confirmed booking.");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}

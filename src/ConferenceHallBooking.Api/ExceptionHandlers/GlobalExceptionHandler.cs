using ConferenceHallBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHallBooking.Api.ExceptionHandlers;

/// <summary>
/// Maps thrown exceptions to RFC 7807 ProblemDetails responses.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string Rfc7807Type = "https://tools.ietf.org/html/rfc7807";

    /// <summary>Converts the exception to a ProblemDetails body and writes the response.</summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        // NOTE: switch-expression cases are evaluated in order. Specific
        // subclasses of BusinessRuleException (ValidationException → 400,
        // NotFoundException/OptionsNotFoundException → 404,
        // InvalidEntityFieldException → 400, InvalidCredentialsException → 401)
        // MUST appear before the generic BusinessRuleException → 409 arm,
        // otherwise they will fall through to 409.
        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Validation error",
                validationEx.Message,
                traceId,
                [("errors", validationEx.Errors)]),

            NotFoundException notFound => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Resource not found",
                notFound.Message,
                traceId),

            OptionsNotFoundException optionsNotFound => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Options not found",
                optionsNotFound.Message,
                traceId),

            InvalidBookingTimeException timeEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Business rule validation error",
                timeEx.Message,
                traceId,
                [("errorCode", timeEx.ErrorCode)]),
            InvalidBaseHourlyRateException rateEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Business rule validation error",
                rateEx.Message,
                traceId,
                [("errorCode", rateEx.ErrorCode)]),
            InvalidEntityFieldException fieldEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Business rule validation error",
                fieldEx.Message,
                traceId,
                [("errorCode", fieldEx.ErrorCode)]),

            InvalidCredentialsException credentialsEx => CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Invalid credentials",
                credentialsEx.Message,
                traceId,
                [("errorCode", credentialsEx.ErrorCode)]),

            HallAlreadyBookedException bookedEx => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Conflict",
                bookedEx.Message,
                traceId,
                [("errorCode", bookedEx.ErrorCode)]),

            BusinessRuleException businessEx => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Business rule violation",
                businessEx.Message,
                traceId,
                [("errorCode", businessEx.ErrorCode)]),

            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred. Please try again later.",
                traceId)
        };

        var statusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        LogException(exception, statusCode);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        string traceId,
        IReadOnlyList<(string Key, object Value)>? extensions = null)
    {
        var problemDetails = new ProblemDetails
        {
            Type = Rfc7807Type,
            Title = title,
            Status = status,
            Detail = detail,
            Extensions = { ["traceId"] = traceId }
        };

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return problemDetails;
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            logger.LogWarning(exception, "Client error: {Message}", exception.Message);
    }
}

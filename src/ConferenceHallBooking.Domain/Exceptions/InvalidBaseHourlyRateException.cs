namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when a base hourly rate is zero or negative.
/// </summary>
public class InvalidBaseHourlyRateException(decimal rate)
    : BusinessRuleException(
        $"Base hourly rate must be greater than zero. Received: {rate}.",
        "INVALID_BASE_HOURLY_RATE");

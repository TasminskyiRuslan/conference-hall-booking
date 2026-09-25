namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Base exception for all domain business rule violations.
/// Carries an error code for programmatic identification.
/// </summary>
public abstract class BusinessRuleException(string message, string errorCode) : Exception(message)
{
    /// <summary>Machine-readable error code exposed to API clients.</summary>
    public string ErrorCode { get; } = errorCode;
}

namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Base exception for all domain business rule violations.
/// Carries an error code for programmatic identification.
/// </summary>
public abstract class BusinessRuleException : Exception
{
    /// <summary>Initializes the exception with a message and an error code.</summary>
    protected BusinessRuleException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>Initializes the exception with an inner (cause) exception.</summary>
    protected BusinessRuleException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>Machine-readable error code exposed to API clients.</summary>
    public string ErrorCode { get; }
}

namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when FluentValidation detects one or more validation errors in a request.
/// Carries a dictionary of field-level error messages.
/// </summary>
public class ValidationException(IDictionary<string, string[]> errors)
    : BusinessRuleException("One or more validation errors occurred.", "VALIDATION_ERROR")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}

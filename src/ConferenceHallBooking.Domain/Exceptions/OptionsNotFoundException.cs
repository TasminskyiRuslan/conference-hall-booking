namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when requested option IDs do not exist in the database.
/// </summary>
public class OptionsNotFoundException(IReadOnlyCollection<Guid> missingIds)
    : BusinessRuleException(
        $"Options not found: {string.Join(", ", missingIds)}.",
        "OPTIONS_NOT_FOUND")
{
    public IReadOnlyCollection<Guid> MissingIds { get; } = missingIds;
}

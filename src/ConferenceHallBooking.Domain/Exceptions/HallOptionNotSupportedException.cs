namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when a booking references options that are not supported by the selected hall.
/// </summary>
public class HallOptionNotSupportedException(Guid hallId, IReadOnlyCollection<Guid> unsupportedOptionIds)
    : BusinessRuleException(
        $"Hall {hallId} does not support options: {string.Join(", ", unsupportedOptionIds)}.",
        "HALL_OPTION_NOT_SUPPORTED")
{
    public IReadOnlyCollection<Guid> UnsupportedOptionIds { get; } = unsupportedOptionIds;
}

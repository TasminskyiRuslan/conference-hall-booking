namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to remove an option from a hall that is used in upcoming bookings.
/// </summary>
public class HallOptionInUseException(Guid hallId, IReadOnlyCollection<Guid> optionIds)
    : BusinessRuleException(
        $"Hall {hallId} has upcoming bookings using options: {string.Join(", ", optionIds)}. These options cannot be removed.",
        "HALL_OPTION_IN_USE")
{
    public Guid HallId { get; } = hallId;
    public IReadOnlyCollection<Guid> OptionIds { get; } = optionIds;
}

namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to create or update a hall with a name that already exists.
/// </summary>
public class HallNameAlreadyExistsException(string name)
    : BusinessRuleException(
        $"A hall with the name '{name}' already exists.",
        "HALL_NAME_ALREADY_EXISTS");

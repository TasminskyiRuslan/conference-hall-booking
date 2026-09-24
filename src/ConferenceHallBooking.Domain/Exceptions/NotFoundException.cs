namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Non-generic base for <see cref="NotFoundException{TEntity}"/>.
/// Allows catching all not-found exceptions without knowing the entity type.
/// </summary>
public class NotFoundException : BusinessRuleException
{
    public NotFoundException(string message)
        : base(message, "NOT_FOUND") { }
}

/// <summary>
/// Thrown when a requested entity is not found in the database.
/// </summary>
public class NotFoundException<TEntity>(object key)
    : NotFoundException($"Entity '{typeof(TEntity).Name}' with key '{key}' was not found.");

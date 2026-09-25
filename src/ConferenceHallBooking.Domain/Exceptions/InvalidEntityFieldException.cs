namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when an entity field violates validation rules (empty name, negative price, etc.).
/// </summary>
public class InvalidEntityFieldException(string entity, string fieldName, string reason)
    : BusinessRuleException(
        $"{entity}.{fieldName} is invalid: {reason}.",
        "INVALID_ENTITY_FIELD")
{
    /// <summary>Name of the entity that failed validation.</summary>
    public string Entity { get; } = entity;
    /// <summary>Name of the field that failed validation.</summary>
    public string FieldName { get; } = fieldName;
}

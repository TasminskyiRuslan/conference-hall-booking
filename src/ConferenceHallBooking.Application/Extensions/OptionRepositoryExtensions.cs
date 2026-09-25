using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;

namespace ConferenceHallBooking.Application.Extensions;

/// <summary>
/// Loads options by IDs and throws if any ID is missing.
/// </summary>
public static class OptionRepositoryExtensions
{
    /// <summary>Loads options by IDs and throws OptionsNotFoundException when any ID is missing.</summary>
    public static async Task<IReadOnlyList<Option>> GetByIdsOrThrowAsync(
        this IOptionRepository repository,
        IReadOnlyCollection<Guid> optionIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = new HashSet<Guid>(optionIds);

        if (distinctIds.Count == 0)
        {
            return [];
        }

        var existing = (await repository.GetByIdsAsync(distinctIds, cancellationToken)).ToList();

        if (existing.Count != distinctIds.Count)
        {
            var existingIds = existing.Select(o => o.Id).ToHashSet();
            var missingIds = distinctIds.Where(id => !existingIds.Contains(id)).ToArray();
            throw new OptionsNotFoundException(missingIds);
        }

        return existing;
    }
}

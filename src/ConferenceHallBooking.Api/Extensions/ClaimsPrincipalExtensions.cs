using System.Security.Claims;

namespace ConferenceHallBooking.Api.Extensions;

/// <summary>
/// Extension methods for reading identity data from the authenticated user.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the authenticated user's ID from the name identifier claim,
    /// or null when the claim is missing or is not a valid GUID.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
}

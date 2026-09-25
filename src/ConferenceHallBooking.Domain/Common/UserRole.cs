namespace ConferenceHallBooking.Domain.Common;

/// <summary>
/// System authorization roles.
/// </summary>
public enum UserRole
{
    /// <summary>Regular customer: can book halls and view own bookings.</summary>
    Customer = 0,

    /// <summary>Administrator: full access, including other users' bookings.</summary>
    Admin = 1
}

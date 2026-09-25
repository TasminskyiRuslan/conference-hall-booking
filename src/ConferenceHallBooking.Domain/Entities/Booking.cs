using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Confirmed booking of a hall for a time slot.
/// Stores a pricing snapshot; owned by the creating user.
/// </summary>
public class Booking
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Foreign key to the booked hall.</summary>
    public Guid HallId { get; private set; }

    /// <summary>Navigation property to the booked hall.</summary>
    public Hall Hall { get; private set; } = null!;

    /// <summary>Foreign key to the booking owner.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Navigation property to the booking owner.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Booking start (inclusive).</summary>
    public DateTimeOffset StartTime { get; private set; }

    /// <summary>Booking end (exclusive).</summary>
    public DateTimeOffset EndTime { get; private set; }

    /// <summary>Hall cost at creation time (pricing snapshot).</summary>
    public decimal HallCost { get; private set; }

    /// <summary>Total cost at creation time (hall + services).</summary>
    public decimal TotalPrice { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private readonly List<BookingOption> _bookingOptions = [];

    /// <summary>
    /// Selected services with prices frozen at booking time.
    /// Options cost = sum of <see cref="BookingOption.PriceAtBooking"/>.
    /// </summary>
    public IReadOnlyCollection<BookingOption> BookingOptions => _bookingOptions;

    private Booking()
    {
    }

    /// <summary>
    /// Creates a booking from a pricing breakdown.
    /// Invariant: hallCost + sum(options.PriceAtBooking) == totalPrice.
    /// </summary>
    /// <param name="hall">Hall to book.</param>
    /// <param name="user">Booking owner.</param>
    /// <param name="startTime">Booking start (inclusive).</param>
    /// <param name="endTime">Booking end (exclusive).</param>
    /// <param name="hallCost">Hall cost from the pricing service.</param>
    /// <param name="totalPrice">Total cost from the pricing service.</param>
    /// <param name="options">Optional services with frozen prices.</param>
    public Booking(
        Hall hall,
        User user,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal hallCost,
        decimal totalPrice,
        IEnumerable<BookingOption>? options = null)
    {
        if (hall is null)
        {
            throw new InvalidEntityFieldException(nameof(Booking), nameof(Hall), "hall cannot be null");
        }

        if (user is null)
        {
            throw new InvalidEntityFieldException(nameof(Booking), nameof(User), "user cannot be null");
        }

        if (endTime <= startTime)
        {
            throw new InvalidBookingTimeException(startTime, endTime);
        }

        if (hallCost < 0)
        {
            throw new InvalidEntityFieldException(nameof(Booking), nameof(HallCost), "hall cost cannot be negative");
        }

        if (totalPrice <= 0)
        {
            throw new InvalidEntityFieldException(nameof(Booking), nameof(TotalPrice), "total price must be greater than zero");
        }

        var optionList = options?.ToList() ?? [];
        var optionsCost = optionList.Sum(o => o.PriceAtBooking);

        if (hallCost + optionsCost != totalPrice)
        {
            throw new InvalidEntityFieldException(
                nameof(Booking),
                nameof(TotalPrice),
                "total price must equal hall cost + options cost");
        }

        Id = Guid.NewGuid();
        Hall = hall;
        HallId = hall.Id;
        User = user;
        UserId = user.Id;
        StartTime = startTime;
        EndTime = endTime;
        HallCost = hallCost;
        TotalPrice = totalPrice;
        CreatedAtUtc = DateTimeOffset.UtcNow;

        if (optionList.Count > 0)
        {
            _bookingOptions.AddRange(optionList);
        }
    }
}

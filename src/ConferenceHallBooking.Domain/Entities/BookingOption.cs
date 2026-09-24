using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>Join entity linking a booking to a selected service option.</summary>
public class BookingOption
{
    /// <summary>Foreign key to the parent booking.</summary>
    public Guid BookingId { get; private set; }

    /// <summary>Navigation property to the parent booking.</summary>
    public Booking Booking { get; private set; } = null!;

    /// <summary>Foreign key to the selected service option.</summary>
    public Guid OptionId { get; private set; }

    /// <summary>Navigation property to the selected service option.</summary>
    public Option Option { get; private set; } = null!;

    /// <summary>Option price frozen at booking time.</summary>
    public decimal PriceAtBooking { get; private set; }

    private BookingOption() { }

    public BookingOption(Option option, decimal priceAtBooking)
    {
        if (option is null)
        {
            throw new InvalidEntityFieldException(nameof(BookingOption), nameof(Option), "option cannot be null");
        }

        if (priceAtBooking < 0)
        {
            throw new InvalidEntityFieldException(nameof(BookingOption), nameof(PriceAtBooking), "price cannot be negative");
        }

        Option = option;
        OptionId = option.Id;
        PriceAtBooking = priceAtBooking;
    }
}

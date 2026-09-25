using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>Join entity linking a hall to an available service option.</summary>
public class HallOption
{
    /// <summary>Foreign key to the hall.</summary>
    public Guid HallId { get; private set; }

    /// <summary>Navigation property to the hall.</summary>
    public Hall Hall { get; private set; } = null!;

    /// <summary>Foreign key to the service option.</summary>
    public Guid OptionId { get; private set; }

    /// <summary>Navigation property to the service option.</summary>
    public Option Option { get; private set; } = null!;

    private HallOption() { }

    /// <summary>Links an option that a hall offers.</summary>
    public HallOption(Option option)
    {
        if (option is null)
        {
            throw new InvalidEntityFieldException(nameof(HallOption), nameof(Option), "option cannot be null");
        }

        Option = option;
        OptionId = option.Id;
    }
}

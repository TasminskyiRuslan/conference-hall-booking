using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Conference hall available for booking.
/// Invariants: name non-empty; capacity and rate positive.
/// </summary>
public class Hall
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Maximum number of people.</summary>
    public int Capacity { get; private set; }

    /// <summary>Base cost per hour in UAH before multipliers.</summary>
    public decimal BaseHourlyRate { get; private set; }

    private readonly List<HallOption> _hallOptions = [];

    /// <summary>Services available in this hall.</summary>
    public IReadOnlyCollection<HallOption> HallOptions => _hallOptions;

    private readonly List<Booking> _bookings = [];

    /// <summary>All bookings for this hall.</summary>
    public IReadOnlyCollection<Booking> Bookings => _bookings;

    private Hall()
    {
    }

    /// <summary>Creates a hall with the given name, capacity and hourly rate.</summary>
    public Hall(string name, int capacity, decimal baseHourlyRate)
    {
        Id = Guid.NewGuid();
        Update(name, capacity, baseHourlyRate);
    }

    /// <summary>
    /// Updates hall properties.
    /// Throws <see cref="InvalidEntityFieldException"/> on invalid input.
    /// </summary>
    public void Update(string name, int capacity, decimal baseHourlyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidEntityFieldException(nameof(Hall), nameof(Name), "name cannot be empty");
        }

        if (capacity <= 0)
        {
            throw new InvalidEntityFieldException(nameof(Hall), nameof(Capacity), "capacity must be greater than zero");
        }

        if (baseHourlyRate <= 0)
        {
            throw new InvalidEntityFieldException(nameof(Hall), nameof(BaseHourlyRate), "base hourly rate must be greater than zero");
        }

        Name = name;
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
    }

    /// <summary>Attaches a service option (idempotent).</summary>
    public void AddOption(Option option)
    {
        if (option is null)
        {
            throw new InvalidEntityFieldException(nameof(Hall), nameof(Option), "option cannot be null");
        }

        if (!_hallOptions.Any(o => o.OptionId == option.Id))
        {
            _hallOptions.Add(new HallOption(option));
        }
    }

    /// <summary>Detaches a service option.</summary>
    public void RemoveOption(Guid optionId)
    {
        var option = _hallOptions.FirstOrDefault(o => o.OptionId == optionId);
        if (option is not null)
        {
            _hallOptions.Remove(option);
        }
    }
}

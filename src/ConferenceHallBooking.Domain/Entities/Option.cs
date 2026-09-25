using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>Bookable service option with a fixed price.</summary>
public class Option
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Fixed price per booking in UAH. Zero is allowed.</summary>
    public decimal Price { get; private set; }

    private readonly List<HallOption> _hallOptions = [];

    /// <summary>Halls that offer this option.</summary>
    public IReadOnlyCollection<HallOption> HallOptions => _hallOptions;

    private Option() { }

    /// <summary>Creates a bookable service option with its price.</summary>
    public Option(string name, decimal price)
    {
        Id = Guid.NewGuid();
        Update(name, price);
    }

    /// <summary>
    /// Updates option properties.
    /// Throws <see cref="InvalidEntityFieldException"/> on invalid input.
    /// </summary>
    public void Update(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidEntityFieldException(nameof(Option), nameof(Name), "name cannot be empty");
        }

        if (price < 0)
        {
            throw new InvalidEntityFieldException(nameof(Option), nameof(Price), "price cannot be negative");
        }

        Name = name;
        Price = price;
    }
}

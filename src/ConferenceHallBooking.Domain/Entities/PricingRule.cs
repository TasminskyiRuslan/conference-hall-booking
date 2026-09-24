using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Time-of-day pricing rule for the pricing service.
/// Window is inclusive of start and exclusive of end.
/// Rules must not overlap; validated when seeding configuration.
/// </summary>
public class PricingRule
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Inclusive window start.</summary>
    public TimeOnly StartTime { get; private set; }

    /// <summary>Exclusive window end.</summary>
    public TimeOnly EndTime { get; private set; }

    /// <summary>Multiplier applied to the base hourly rate.</summary>
    public decimal Multiplier { get; private set; }

    private PricingRule() { }

    /// <summary>
    /// Creates a pricing rule.
    /// Throws InvalidEntityFieldException on invalid input.
    /// </summary>
    public PricingRule(TimeOnly startTime, TimeOnly endTime, decimal multiplier)
    {
        if (endTime <= startTime)
        {
            throw new InvalidEntityFieldException(
                nameof(PricingRule), nameof(EndTime), "end time must be greater than start time");
        }

        if (multiplier <= 0)
        {
            throw new InvalidEntityFieldException(
                nameof(PricingRule), nameof(Multiplier), "multiplier must be greater than zero");
        }

        Id = Guid.NewGuid();
        StartTime = startTime;
        EndTime = endTime;
        Multiplier = multiplier;
    }
}

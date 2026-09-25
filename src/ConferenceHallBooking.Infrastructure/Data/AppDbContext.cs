using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Data;

/// <summary>
/// Application database context. Applies all entity configurations from this assembly.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Creates the context with the given options.</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>Conference halls.</summary>
    public DbSet<Hall> Halls => Set<Hall>();
    /// <summary>Bookable service options.</summary>
    public DbSet<Option> Options => Set<Option>();
    /// <summary>Hall-to-option links.</summary>
    public DbSet<HallOption> HallOptions => Set<HallOption>();
    /// <summary>Confirmed bookings.</summary>
    public DbSet<Booking> Bookings => Set<Booking>();
    /// <summary>Booking-to-option links with frozen prices.</summary>
    public DbSet<BookingOption> BookingOptions => Set<BookingOption>();
    /// <summary>System users.</summary>
    public DbSet<User> Users => Set<User>();
    /// <summary>Time-of-day pricing rules.</summary>
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();

    /// <summary>Applies entity configurations from this assembly.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

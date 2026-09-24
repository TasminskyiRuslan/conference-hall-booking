using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Data;

/// <summary>
/// Application database context. Applies all entity configurations from this assembly.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<HallOption> HallOptions => Set<HallOption>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingOption> BookingOptions => Set<BookingOption>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

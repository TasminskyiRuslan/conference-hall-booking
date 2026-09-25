using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the Booking entity.
/// Overlap prevention uses a PostgreSQL EXCLUDE constraint added in a later migration.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <summary>Configures keys, relations and decimal precision for Booking.</summary>
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.HallCost)
            .HasPrecision(18, 2);

        builder.Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(b => b.StartTime)
            .IsRequired();

        builder.Property(b => b.EndTime)
            .IsRequired();

        builder.Property(b => b.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(b => b.Hall)
            .WithMany(h => h.Bookings)
            .HasForeignKey(b => b.HallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.HallId, b.StartTime, b.EndTime });

        builder.Navigation(b => b.BookingOptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

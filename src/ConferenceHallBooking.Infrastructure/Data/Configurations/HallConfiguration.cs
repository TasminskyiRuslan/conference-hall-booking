using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the Hall entity.
/// </summary>
public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    /// <summary>Configures keys, relations and decimal precision for Hall.</summary>
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(h => h.Name)
            .IsUnique();

        builder.Property(h => h.Capacity)
            .IsRequired();

        builder.Property(h => h.BaseHourlyRate)
            .HasPrecision(18, 2);

        builder.Navigation(h => h.HallOptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(h => h.Bookings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

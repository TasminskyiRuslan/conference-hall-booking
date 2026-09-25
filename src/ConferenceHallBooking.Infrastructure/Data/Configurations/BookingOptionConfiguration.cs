using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the BookingOption join entity.
/// </summary>
public class BookingOptionConfiguration : IEntityTypeConfiguration<BookingOption>
{
    /// <summary>Configures keys, relations and decimal precision for BookingOption.</summary>
    public void Configure(EntityTypeBuilder<BookingOption> builder)
    {
        builder.HasKey(bo => new { bo.BookingId, bo.OptionId });

        builder.Property(bo => bo.PriceAtBooking)
            .HasPrecision(18, 2);

        builder.HasOne(bo => bo.Booking)
            .WithMany(b => b.BookingOptions)
            .HasForeignKey(bo => bo.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bo => bo.Option)
            .WithMany()
            .HasForeignKey(bo => bo.OptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

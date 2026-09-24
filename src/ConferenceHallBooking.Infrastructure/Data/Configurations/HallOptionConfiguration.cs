using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the HallOption join entity.
/// </summary>
public class HallOptionConfiguration : IEntityTypeConfiguration<HallOption>
{
    public void Configure(EntityTypeBuilder<HallOption> builder)
    {
        builder.HasKey(ho => new { ho.HallId, ho.OptionId });

        builder.HasOne(ho => ho.Hall)
            .WithMany(h => h.HallOptions)
            .HasForeignKey(ho => ho.HallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ho => ho.Option)
            .WithMany(o => o.HallOptions)
            .HasForeignKey(ho => ho.OptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

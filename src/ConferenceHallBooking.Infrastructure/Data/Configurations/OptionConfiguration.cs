using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the Option entity.
/// </summary>
public class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(o => o.Name)
            .IsUnique();

        builder.Property(o => o.Price)
            .HasPrecision(18, 2);

        builder.Navigation(o => o.HallOptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

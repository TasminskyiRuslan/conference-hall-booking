using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the PricingRule entity.
/// </summary>
public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.ToTable("PricingRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Multiplier)
            .HasPrecision(18, 2);
    }
}

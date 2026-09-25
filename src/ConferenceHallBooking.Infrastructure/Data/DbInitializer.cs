using ConferenceHallBooking.Application.Interfaces;
using ConferenceHallBooking.Domain.Common;
using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConferenceHallBooking.Infrastructure.Data;

/// <summary>
/// Applies migrations and seeds initial data on first run.
/// </summary>
public class DbInitializer(
    AppDbContext context,
    IConfiguration configuration,
    ILogger<DbInitializer> logger) : IDbInitializer
{
    private const string AdminSeedSection = "AdminSeed";

    /// <summary>Applies pending migrations and seeds reference data when missing.</summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Applying pending database migrations...");
            await context.Database.MigrateAsync(cancellationToken);

            await SeedAdminAsync(cancellationToken);
            await SeedPricingRulesAsync(cancellationToken);
            await SeedHallsAndOptionsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedPricingRulesAsync(CancellationToken cancellationToken)
    {
        if (await context.PricingRules.AnyAsync(cancellationToken))
        {
            return;
        }

        var seeds = configuration
            .GetSection("PricingSettings:Rules")
            .Get<List<PricingRuleSeed>>() ?? [];

        if (seeds.Count == 0)
        {
            logger.LogWarning(
                "No pricing rules found under {Section}. PricingRules table left empty; multipliers default to 1.0.",
                "PricingSettings:Rules");
            return;
        }

        var rules = seeds
            .Select(s => new PricingRule(s.StartTime, s.EndTime, s.Multiplier))
            .ToList();

        await context.PricingRules.AddRangeAsync(rules, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} pricing rule(s) from configuration.", rules.Count);
    }

    private async Task SeedHallsAndOptionsAsync(CancellationToken cancellationToken)
    {
        if (await context.Halls.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded. Skipping halls and options.");
            return;
        }

        logger.LogInformation("Seeding halls and options...");

        var projector = new Option("Проєктор", 500m);
        var wifi = new Option("Wi-Fi", 300m);
        var sound = new Option("Звук", 700m);

        var hallA = new Hall("Зал А", 50, 2000m);
        var hallB = new Hall("Зал B", 100, 3500m);
        var hallC = new Hall("Зал C", 30, 1500m);

        await context.Options.AddRangeAsync([projector, wifi, sound], cancellationToken);
        await context.Halls.AddRangeAsync([hallA, hallB, hallC], cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        hallA.AddOption(projector);
        hallA.AddOption(wifi);
        hallA.AddOption(sound);

        hallB.AddOption(projector);
        hallB.AddOption(wifi);
        hallB.AddOption(sound);

        hallC.AddOption(projector);
        hallC.AddOption(wifi);
        hallC.AddOption(sound);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Halls and options seeded successfully.");
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        var email = configuration[$"{AdminSeedSection}:Email"];
        var password = configuration[$"{AdminSeedSection}:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Admin user was not created. Provide {Section}:Email and {Section}:Password (e.g. via environment variables).",
                AdminSeedSection,
                AdminSeedSection);
            return;
        }

        if (await context.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var admin = new User(email, passwordHash, "Administrator", UserRole.Admin);

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Admin user '{Email}' seeded successfully.", email);
    }

    private sealed record PricingRuleSeed(TimeOnly StartTime, TimeOnly EndTime, decimal Multiplier);
}

using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.PostgresTests;

/// <summary>
/// Connection factory and migration gate for tests that need a live PostgreSQL instance.
/// Tests no-op unless POSTGRES_TEST_CONNECTION points at a database.
/// </summary>
internal static class PostgresTestDatabase
{
    private static readonly SemaphoreSlim MigrationLock = new(1, 1);

    /// <summary>Live database connection string; null when POSTGRES_TEST_CONNECTION is unset.</summary>
    public static string? ConnectionString { get; } =
        Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION");

    /// <summary>True when the tests are pointed at a live database.</summary>
    public static bool Enabled => ConnectionString is not null;

    /// <summary>Creates a context bound to the live database.</summary>
    public static AppDbContext CreateContext()
    {
        var connectionString = ConnectionString
            ?? throw new InvalidOperationException("POSTGRES_TEST_CONNECTION is not set");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>Applies pending migrations; safe on an empty database and serialized across test classes.</summary>
    public static async Task EnsureMigratedAsync()
    {
        await MigrationLock.WaitAsync();
        try
        {
            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }
        finally
        {
            MigrationLock.Release();
        }
    }
}

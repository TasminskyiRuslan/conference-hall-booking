using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Features.Bookings.Handlers;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConferenceHallBooking.PostgresTests;

/// <summary>
/// Live-PostgreSQL verification of the double-booking guarantee: the Bookings_NoOverlap
/// exclusion constraint and the create-booking handler under concurrency.
/// Skipped when POSTGRES_TEST_CONNECTION is not set.
/// </summary>
public class BookingOverlapTests
{
    private static readonly DateTimeOffset SlotStart = new(2032, 1, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExclusionConstraint_WhenOverlappingBookingInserted_RejectsWith23P01()
    {
        if (!PostgresTestDatabase.Enabled)
        {
            return;
        }

        await PostgresTestDatabase.EnsureMigratedAsync();

        var stamp = Guid.NewGuid().ToString("N")[..8];
        Guid hallId;
        Guid userId;

        await using (var setupContext = PostgresTestDatabase.CreateContext())
        {
            var hall = new Hall($"Overlap test hall {stamp}", 10, 100m);
            var user = new User($"overlap.{stamp}@test.local", "hash", "Overlap Tester");
            setupContext.Halls.Add(hall);
            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();
            hallId = hall.Id;
            userId = user.Id;
        }

        try
        {
            await using (var firstContext = PostgresTestDatabase.CreateContext())
            {
                var hall = await firstContext.Halls.SingleAsync(h => h.Id == hallId);
                var user = await firstContext.Users.SingleAsync(u => u.Id == userId);
                firstContext.Bookings.Add(new Booking(hall, user, SlotStart, SlotStart.AddHours(2), 200m, 200m));
                await firstContext.SaveChangesAsync();
            }

            await using (var secondContext = PostgresTestDatabase.CreateContext())
            {
                var hall = await secondContext.Halls.SingleAsync(h => h.Id == hallId);
                var user = await secondContext.Users.SingleAsync(u => u.Id == userId);
                secondContext.Bookings.Add(new Booking(hall, user, SlotStart.AddHours(1), SlotStart.AddHours(3), 200m, 200m));

                Func<Task> act = () => secondContext.SaveChangesAsync();
                var assertion = await act.Should().ThrowAsync<Exception>();

                ContainsExclusionViolation(assertion.Which).Should().BeTrue(
                    "the Bookings_NoOverlap constraint must reject the overlapping insert");
            }
        }
        finally
        {
            await CleanupAsync(hallId, userId);
        }
    }

    [Fact]
    public async Task ParallelCreateBooking_WhenSameSlot_AllowsExactlyOneWinner()
    {
        if (!PostgresTestDatabase.Enabled)
        {
            return;
        }

        await PostgresTestDatabase.EnsureMigratedAsync();

        var stamp = Guid.NewGuid().ToString("N")[..8];
        Guid hallId;
        Guid userId;

        await using (var setupContext = PostgresTestDatabase.CreateContext())
        {
            var hall = new Hall($"Race test hall {stamp}", 10, 100m);
            var user = new User($"race.{stamp}@test.local", "hash", "Race Tester");
            setupContext.Halls.Add(hall);
            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();
            hallId = hall.Id;
            userId = user.Id;
        }

        try
        {
            var ready = 0;
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var attempts = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ =>
                Task.Run<(bool Won, Exception? Error)>(async () =>
                {
                    if (Interlocked.Increment(ref ready) == 4)
                    {
                        gate.SetResult();
                    }

                    await gate.Task;

                    await using var context = PostgresTestDatabase.CreateContext();
                    var handler = new CreateBookingCommandHandler(
                        new BookingRepository(context),
                        new HallRepository(context),
                        new OptionRepository(context),
                        new UserRepository(context),
                        new PricingService(new PricingRuleRepository(context)),
                        new UnitOfWork(context));

                    var command = new CreateBookingCommand(hallId, userId, SlotStart, 2m, null);

                    try
                    {
                        await handler.Handle(command, CancellationToken.None);
                        return (true, (Exception?)null);
                    }
                    catch (Exception exception)
                    {
                        return (false, exception);
                    }
                })));

            attempts.Count(attempt => attempt.Won).Should().Be(
                1, "exactly one concurrent booking must win the slot");

            foreach (var failure in attempts.Where(attempt => !attempt.Won))
            {
                var isAcceptable = failure.Error is HallAlreadyBookedException
                    || ContainsExclusionViolation(failure.Error!);

                isAcceptable.Should().BeTrue(
                    $"concurrent failure must be an overlap rejection but was: {failure.Error?.GetType().Name}");
            }
        }
        finally
        {
            await CleanupAsync(hallId, userId);
        }
    }

    [Fact]
    public async Task BackToBackCreateBooking_WhenAdjacentSlots_BothSucceed()
    {
        if (!PostgresTestDatabase.Enabled)
        {
            return;
        }

        await PostgresTestDatabase.EnsureMigratedAsync();

        var stamp = Guid.NewGuid().ToString("N")[..8];
        Guid hallId;
        Guid userId;

        await using (var setupContext = PostgresTestDatabase.CreateContext())
        {
            var hall = new Hall($"Back-to-back test hall {stamp}", 10, 100m);
            var user = new User($"backtoback.{stamp}@test.local", "hash", "Back To Back Tester");
            setupContext.Halls.Add(hall);
            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();
            hallId = hall.Id;
            userId = user.Id;
        }

        try
        {
            await using var context = PostgresTestDatabase.CreateContext();
            var handler = new CreateBookingCommandHandler(
                new BookingRepository(context),
                new HallRepository(context),
                new OptionRepository(context),
                new UserRepository(context),
                new PricingService(new PricingRuleRepository(context)),
                new UnitOfWork(context));

            var first = new CreateBookingCommand(hallId, userId, SlotStart, 2m, null);
            var second = new CreateBookingCommand(hallId, userId, SlotStart.AddHours(2), 2m, null);

            Func<Task> bookFirst = () => handler.Handle(first, CancellationToken.None);
            await bookFirst.Should().NotThrowAsync(
                "the [10:00,12:00) booking must be accepted");

            Func<Task> bookSecond = () => handler.Handle(second, CancellationToken.None);
            await bookSecond.Should().NotThrowAsync(
                "the back-to-back [12:00,14:00) booking must not be treated as an overlap");
        }
        finally
        {
            await CleanupAsync(hallId, userId);
        }
    }

    private static bool ContainsExclusionViolation(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation })
            {
                return true;
            }
        }

        return false;
    }

    private static async Task CleanupAsync(Guid hallId, Guid userId)
    {
        await using var context = PostgresTestDatabase.CreateContext();

        var bookings = await context.Bookings
            .Where(booking => booking.HallId == hallId)
            .ToListAsync();
        context.Bookings.RemoveRange(bookings);

        var hall = await context.Halls.SingleOrDefaultAsync(h => h.Id == hallId);
        if (hall is not null)
        {
            context.Halls.Remove(hall);
        }

        var user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is not null)
        {
            context.Users.Remove(user);
        }

        await context.SaveChangesAsync();
    }
}

using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConferenceHallBooking.UnitTests.Infrastructure.Data;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_WithUniqueViolation_ShouldThrowUniqueConstraintViolationException()
    {
        var unitOfWork = CreateUnitOfWork(
            new DbUpdateException("save failed", Postgres(PostgresErrorCodes.UniqueViolation)));

        var act = () => unitOfWork.SaveChangesAsync(CancellationToken.None);

        var ex = await act.Should().ThrowAsync<UniqueConstraintViolationException>();
        ex.Which.InnerException.Should().BeOfType<DbUpdateException>();
        ex.Which.ErrorCode.Should().Be("UNIQUE_CONSTRAINT_VIOLATION");
    }

    [Fact]
    public async Task SaveChangesAsync_WithForeignKeyViolation_ShouldThrowForeignKeyConstraintViolationException()
    {
        var unitOfWork = CreateUnitOfWork(
            new DbUpdateException("save failed", Postgres(PostgresErrorCodes.ForeignKeyViolation)));

        var act = () => unitOfWork.SaveChangesAsync(CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ForeignKeyConstraintViolationException>();
        ex.Which.ErrorCode.Should().Be("FOREIGN_KEY_VIOLATION");
    }

    [Fact]
    public async Task SaveChangesAsync_WithExclusionViolation_ShouldThrowBookingOverlapException()
    {
        var unitOfWork = CreateUnitOfWork(
            new DbUpdateException("save failed", Postgres(PostgresErrorCodes.ExclusionViolation)));

        var act = () => unitOfWork.SaveChangesAsync(CancellationToken.None);

        var ex = await act.Should().ThrowAsync<BookingOverlapException>();
        ex.Which.ErrorCode.Should().Be("HALL_ALREADY_BOOKED");
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutPostgresException_ShouldRethrowOriginalDbUpdateException()
    {
        var original = new DbUpdateException("save failed");
        var unitOfWork = CreateUnitOfWork(original);

        var act = () => unitOfWork.SaveChangesAsync(CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DbUpdateException>();
        ex.Which.Should().BeSameAs(original);
    }

    private static PostgresException Postgres(string sqlState) =>
        new("error", "ERROR", "ERROR", sqlState);

    private static UnitOfWork CreateUnitOfWork(Exception toThrow) =>
        new(new ThrowingDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            toThrow));

    private sealed class ThrowingDbContext(
        DbContextOptions<AppDbContext> options,
        Exception exception) : AppDbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw exception;
    }
}

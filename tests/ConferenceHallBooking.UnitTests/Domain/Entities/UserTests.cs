using ConferenceHallBooking.Domain.Common;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var user = new User("test@email.com", "hash123", "John Doe");

        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be("test@email.com");
        user.PasswordHash.Should().Be("hash123");
        user.FullName.Should().Be("John Doe");
        user.Role.Should().Be(UserRole.Customer);
        user.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithCustomRole_ShouldSetRole()
    {
        var user = new User("test@email.com", "hash", "John", UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new User("", "hash", "John");

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(User.Email));
    }

    [Fact]
    public void Constructor_WithWhitespaceEmail_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new User("  ", "hash", "John");

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(User.Email));
    }

    [Fact]
    public void Constructor_WithEmptyPasswordHash_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new User("test@email.com", "", "John");

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(User.PasswordHash));
    }

    [Fact]
    public void Constructor_WithEmptyFullName_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new User("test@email.com", "hash", "");

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(User.FullName));
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new User(null!, "hash", "John");

        act.Should().Throw<InvalidEntityFieldException>();
    }
}

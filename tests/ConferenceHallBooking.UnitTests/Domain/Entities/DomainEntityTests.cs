using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Domain.Entities;

public class DomainEntityTests
{
    #region Hall

    [Fact]
    public void Hall_Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var hall = new Hall("Conference Room A", 50, 100m);

        hall.Id.Should().NotBe(Guid.Empty);
        hall.Name.Should().Be("Conference Room A");
        hall.Capacity.Should().Be(50);
        hall.BaseHourlyRate.Should().Be(100m);
    }

    [Fact]
    public void Hall_Constructor_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("", 50, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Name));
    }

    [Fact]
    public void Hall_Constructor_WithWhitespaceName_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("   ", 50, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Name));
    }

    [Fact]
    public void Hall_Constructor_WithZeroCapacity_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", 0, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Capacity));
    }

    [Fact]
    public void Hall_Constructor_WithNegativeCapacity_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", -1, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Capacity));
    }

    [Fact]
    public void Hall_Constructor_WithZeroBaseHourlyRate_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", 50, 0m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.BaseHourlyRate));
    }

    [Fact]
    public void Hall_Constructor_WithNegativeBaseHourlyRate_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", 50, -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.BaseHourlyRate));
    }

    [Fact]
    public void Hall_Update_WithValidData_ShouldChangeProperties()
    {
        var hall = new Hall("Conference Room A", 50, 100m);

        hall.Update("Conference Room B", 100, 200m);

        hall.Name.Should().Be("Conference Room B");
        hall.Capacity.Should().Be(100);
        hall.BaseHourlyRate.Should().Be(200m);
    }

    [Fact]
    public void Hall_Update_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var hall = new Hall("Conference Room A", 50, 100m);

        var act = () => hall.Update("", 50, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Name));
    }

    [Fact]
    public void Hall_AddOption_ShouldAddToHallOptions()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var option = new Option("Projector", 50m);

        hall.AddOption(option);

        hall.HallOptions.Should().ContainSingle(o => o.OptionId == option.Id);
    }

    [Fact]
    public void Hall_AddOption_WithDuplicateId_ShouldNotAddDuplicate()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var option = new Option("Projector", 50m);

        hall.AddOption(option);
        hall.AddOption(option);

        hall.HallOptions.Should().ContainSingle(o => o.OptionId == option.Id);
    }

    [Fact]
    public void Hall_RemoveOption_ShouldRemoveFromHallOptions()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var option = new Option("Projector", 50m);
        hall.AddOption(option);

        hall.RemoveOption(option.Id);

        hall.HallOptions.Should().BeEmpty();
    }

    #endregion

    #region Booking

    [Fact]
    public void Booking_Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var booking = new Booking(hall, user, startTime, endTime, 300m, 300m);

        booking.Id.Should().NotBe(Guid.Empty);
        booking.UserId.Should().Be(user.Id);
        booking.StartTime.Should().Be(startTime);
        booking.EndTime.Should().Be(endTime);
        booking.HallCost.Should().Be(300m);
        booking.TotalPrice.Should().Be(300m);
        booking.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Booking_Constructor_WithNullHall_ShouldThrowInvalidEntityFieldException()
    {
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var act = () => new Booking(null!, user, startTime, endTime, 300m, 300m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Booking.Hall));
    }

    [Fact]
    public void Booking_Constructor_WithNullUser_ShouldThrowInvalidEntityFieldException()
    {
        var hall = new Hall("Hall", 50, 100m);
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var act = () => new Booking(hall, null!, startTime, endTime, 300m, 300m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Booking.User));
    }

    [Fact]
    public void Booking_Constructor_WithEndTimeBeforeStartTime_ShouldThrowInvalidBookingTimeException()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(-1);

        var act = () => new Booking(hall, user, startTime, endTime, 300m, 300m);

        act.Should().Throw<InvalidBookingTimeException>();
    }

    [Fact]
    public void Booking_Constructor_WithEndTimeEqualToStartTime_ShouldThrowInvalidBookingTimeException()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var time = DateTimeOffset.UtcNow;

        var act = () => new Booking(hall, user, time, time, 300m, 300m);

        act.Should().Throw<InvalidBookingTimeException>();
    }

    [Fact]
    public void Booking_Constructor_WithTotalPriceZero_ShouldThrowInvalidEntityFieldException()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var act = () => new Booking(hall, user, startTime, endTime, 0m, 0m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Booking.TotalPrice));
    }

    [Fact]
    public void Booking_Constructor_WithTotalPriceNegative_ShouldThrowInvalidEntityFieldException()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var act = () => new Booking(hall, user, startTime, endTime, -100m, -100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Booking.HallCost));
    }

    [Fact]
    public void Booking_Constructor_WithMismatchedPriceBreakdown_ShouldThrowInvalidEntityFieldException()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);
        var options = new List<BookingOption> { new(new Option("Premium", 500m), 500m) };

        var act = () => new Booking(hall, user, startTime, endTime, 100m, 100m, options);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Booking.TotalPrice));
    }

    [Fact]
    public void Booking_Constructor_WithOptions_ShouldStoreInBookingOptions()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);
        var optionA = new Option("Projector", 50m);
        var optionB = new Option("Wi-Fi", 30m);
        var options = new List<BookingOption>
        {
            new(optionA, 50m),
            new(optionB, 30m)
        };

        var booking = new Booking(hall, user, startTime, endTime, 300m, 380m, options);

        booking.BookingOptions.Should().HaveCount(2);
        booking.HallCost.Should().Be(300m);
        booking.TotalPrice.Should().Be(380m);
        (booking.HallCost + booking.BookingOptions.Sum(o => o.PriceAtBooking))
            .Should().Be(booking.TotalPrice);
    }

    [Fact]
    public void Booking_Constructor_WithoutOptions_ShouldCreateEmptyBookingOptions()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var booking = new Booking(hall, user, startTime, endTime, 300m, 300m);

        booking.BookingOptions.Should().BeEmpty();
    }

    [Fact]
    public void Booking_Constructor_ShouldSetHallAndUser()
    {
        var hall = new Hall("Hall", 50, 100m);
        var user = new User("owner@email.com", "hash", "Owner");
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var booking = new Booking(hall, user, startTime, endTime, 300m, 300m);

        booking.HallId.Should().Be(hall.Id);
        booking.Hall.Should().BeSameAs(hall);
        booking.UserId.Should().Be(user.Id);
        booking.User.Should().BeSameAs(user);
    }

    #endregion

    #region Option

    [Fact]
    public void Option_Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var option = new Option("Projector", 50m);

        option.Id.Should().NotBe(Guid.Empty);
        option.Name.Should().Be("Projector");
        option.Price.Should().Be(50m);
    }

    [Fact]
    public void Option_Constructor_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Option("", 50m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Name));
    }

    [Fact]
    public void Option_Constructor_WithNegativePrice_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Option("Projector", -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Price));
    }

    [Fact]
    public void Option_Update_WithValidData_ShouldChangeProperties()
    {
        var option = new Option("Projector", 50m);

        option.Update("Screen", 75m);

        option.Name.Should().Be("Screen");
        option.Price.Should().Be(75m);
    }

    [Fact]
    public void Option_Update_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var option = new Option("Projector", 50m);

        var act = () => option.Update("", 50m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Name));
    }

    [Fact]
    public void Option_Update_WithNegativePrice_ShouldThrowInvalidEntityFieldException()
    {
        var option = new Option("Projector", 50m);

        var act = () => option.Update("Projector", -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Price));
    }

    #endregion

    #region BookingOption

    [Fact]
    public void BookingOption_Constructor_WithValidData_ShouldSetOptionAndPriceAtBooking()
    {
        var option = new Option("Projector", 50m);

        var bookingOption = new BookingOption(option, 50m);

        bookingOption.Option.Should().BeSameAs(option);
        bookingOption.OptionId.Should().Be(option.Id);
        bookingOption.PriceAtBooking.Should().Be(50m);
    }

    [Fact]
    public void BookingOption_Constructor_WithNegativePriceAtBooking_ShouldThrowInvalidEntityFieldException()
    {
        var option = new Option("Projector", 50m);

        var act = () => new BookingOption(option, -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(BookingOption.PriceAtBooking));
    }

    #endregion
}

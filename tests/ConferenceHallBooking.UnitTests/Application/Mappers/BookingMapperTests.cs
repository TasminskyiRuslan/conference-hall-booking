using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Mappers;

public class BookingMapperTests
{
    private static User NewUser() =>
        new($"owner-{Guid.NewGuid():N}@example.com", "hash", "Booking Owner");

    [Fact]
    public void MapToResponse_WhenBookingHasNoOptions_ShouldMapScalarProperties()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new Booking(hall, user, startTime, startTime.AddHours(2), 200m, 200m);

        var result = BookingMapper.MapToResponse(booking);

        result.Id.Should().Be(booking.Id);
        result.HallId.Should().Be(hall.Id);
        result.HallName.Should().Be("Conference Room A");
        result.HallCapacity.Should().Be(50);
        result.HallBaseHourlyRate.Should().Be(100m);
        result.StartTime.Should().Be(startTime);
        result.EndTime.Should().Be(startTime.AddHours(2));
        result.DurationHours.Should().Be(2m);
        result.SelectedOptions.Should().BeEmpty();
        result.HallCost.Should().Be(200m);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(200m);
    }

    [Fact]
    public void MapToResponse_WhenBookingHasOptions_ShouldMapOptionDetails()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var projector = new Option("Projector", 50m);
        var wifi = new Option("Wi-Fi", 30m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new Booking(
            hall, user, startTime, startTime.AddHours(3), 300m, 380m,
            [new BookingOption(projector, 50m), new BookingOption(wifi, 30m)]);

        var result = BookingMapper.MapToResponse(booking);

        result.SelectedOptions.Should().HaveCount(2);
        result.SelectedOptions.Should().Contain(o =>
            o.Id == projector.Id && o.Name == "Projector" && o.Price == 50m);
        result.SelectedOptions.Should().Contain(o =>
            o.Id == wifi.Id && o.Name == "Wi-Fi" && o.Price == 30m);
        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(80m);
        result.TotalCost.Should().Be(380m);
    }

    [Fact]
    public void MapToResponse_ShouldComputeDurationHoursFromStartAndEnd()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new Booking(hall, user, startTime, startTime.AddMinutes(90), 150m, 150m);

        var result = BookingMapper.MapToResponse(booking);

        result.DurationHours.Should().Be(1.5m);
    }

    [Fact]
    public void MapToResponse_ShouldNotMutateSourceBooking()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var option = new Option("Projector", 50m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new Booking(
            hall, user, startTime, startTime.AddHours(3), 300m, 350m,
            [new BookingOption(option, 50m)]);

        BookingMapper.MapToResponse(booking);

        booking.StartTime.Should().Be(startTime);
        booking.EndTime.Should().Be(startTime.AddHours(3));
        booking.HallCost.Should().Be(300m);
        booking.TotalPrice.Should().Be(350m);
        booking.BookingOptions.Should().HaveCount(1);
        hall.Name.Should().Be("Conference Room A");
        hall.HallOptions.Should().BeEmpty();
    }
}

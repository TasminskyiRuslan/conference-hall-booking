using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Mappers;

public class HallMapperTests
{
    [Fact]
    public void MapToResponse_WhenHallHasNoOptions_ShouldMapScalarProperties()
    {
        var hall = new Hall("Grand Hall", 100, 250m);

        var response = HallMapper.MapToResponse(hall);

        response.Id.Should().Be(hall.Id);
        response.Name.Should().Be("Grand Hall");
        response.Capacity.Should().Be(100);
        response.BaseHourlyRate.Should().Be(250m);
        response.Options.Should().BeEmpty();
    }

    [Fact]
    public void MapToResponse_WhenHallHasOptions_ShouldMapOptionDetails()
    {
        var hall = new Hall("Grand Hall", 100, 250m);
        var projector = new Option("Projector", 50m);
        var wifi = new Option("Wi-Fi", 30m);
        hall.AddOption(projector);
        hall.AddOption(wifi);

        var response = HallMapper.MapToResponse(hall);

        response.Options.Should().HaveCount(2);
        response.Options.Should().Contain(o => o.Id == projector.Id && o.Name == "Projector" && o.Price == 50m);
        response.Options.Should().Contain(o => o.Id == wifi.Id && o.Name == "Wi-Fi" && o.Price == 30m);
    }

    [Fact]
    public void MapToResponse_ShouldNotMutateSourceHall()
    {
        var hall = new Hall("Grand Hall", 100, 250m);
        hall.AddOption(new Option("Projector", 50m));
        var optionCountBefore = hall.HallOptions.Count;

        _ = HallMapper.MapToResponse(hall);

        hall.Name.Should().Be("Grand Hall");
        hall.HallOptions.Count.Should().Be(optionCountBefore);
    }
}

using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Domain.Entities;

public class HallOptionTests
{
    [Fact]
    public void Constructor_WithValidOption_ShouldMapIdAndOption()
    {
        var option = new Option("Projector", 50m);

        var hallOption = new HallOption(option);

        hallOption.OptionId.Should().Be(option.Id);
        hallOption.Option.Should().BeSameAs(option);
    }

    [Fact]
    public void Constructor_WithNullOption_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new HallOption(null!);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(HallOption.Option));
    }
}

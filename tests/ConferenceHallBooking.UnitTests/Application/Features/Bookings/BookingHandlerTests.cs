using ConferenceHallBooking.Application.Common.Models;
using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Features.Bookings.Handlers;
using ConferenceHallBooking.Application.Features.Bookings.Queries;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Domain.Common;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Bookings;

public class BookingHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPricingService _pricingService = Substitute.For<IPricingService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateBookingCommandHandler CreateBookingHandler() =>
        new(_bookingRepository, _hallRepository, _optionRepository, _userRepository, _pricingService, _unitOfWork);

    private static User NewUser(UserRole role = UserRole.Customer) =>
        new($"user-{Guid.NewGuid():N}@example.com", "hash", "Test User", role);

    #region CreateBookingCommandHandler

    [Fact]
    public async Task CreateBooking_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = CreateBookingHandler();

        var hallId = Guid.NewGuid();
        var command = new CreateBookingCommand(hallId, Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 2m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();

        await _bookingRepository.DidNotReceive().HasOverlappingBookingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBooking_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = CreateBookingHandler();

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var userId = Guid.NewGuid();
        var command = new CreateBookingCommand(hallId, userId, DateTimeOffset.UtcNow.AddDays(1), 2m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<User>>();

        await _bookingRepository.DidNotReceive().HasOverlappingBookingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBooking_WhenTimeSlotIsOverlapping_ShouldThrowWithoutSaving()
    {
        var handler = CreateBookingHandler();

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreateBookingCommand(hallId, user.Id, startTime, 3m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _bookingRepository.HasOverlappingBookingAsync(
                hall.Id, startTime, startTime.AddHours(3), Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallAlreadyBookedException>();

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBooking_WhenSelectedOptionIsNotSupportedByHall_ShouldThrowWithoutCallingOptionRepository()
    {
        var handler = CreateBookingHandler();

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var unsupportedOptionId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreateBookingCommand(hallId, user.Id, startTime, 2m, [unsupportedOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _bookingRepository.HasOverlappingBookingAsync(
                hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallOptionNotSupportedException>();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBooking_WhenOptionDoesNotExistInDb_ShouldThrowWithoutSaving()
    {
        var handler = CreateBookingHandler();

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var option = new Option("Missing Option", 10m);
        hall.AddOption(option);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreateBookingCommand(hallId, user.Id, startTime, 2m, [option.Id]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _bookingRepository.HasOverlappingBookingAsync(
                hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(option.Id)),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OptionsNotFoundException>();

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBooking_WithoutOptions_ShouldCreateBookingAndReturnCompleteResponse()
    {
        var handler = CreateBookingHandler();

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 2m;
        var endTime = startTime.AddHours((double)durationHours);

        var command = new CreateBookingCommand(hallId, user.Id, startTime, durationHours, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _bookingRepository.HasOverlappingBookingAsync(
                hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);
        _pricingService.CalculatePriceAsync(
                hall.BaseHourlyRate,
                Arg.Is<IReadOnlyCollection<decimal>?>(prices => prices != null && prices.Count == 0),
                startTime,
                endTime,
                Arg.Any<CancellationToken>())
            .Returns(new PricingResult(200m, 0m, 200m));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.HallId.Should().Be(hall.Id);
        result.HallName.Should().Be("Conference Room A");
        result.HallCapacity.Should().Be(50);
        result.HallBaseHourlyRate.Should().Be(100m);
        result.StartTime.Should().Be(startTime);
        result.EndTime.Should().Be(endTime);
        result.DurationHours.Should().Be(2m);
        result.SelectedOptions.Should().BeEmpty();
        result.HallCost.Should().Be(200m);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(200m);

        await _bookingRepository.Received(1).AddAsync(
            Arg.Is<Booking>(b =>
                b.HallId == hall.Id &&
                b.UserId == user.Id &&
                b.StartTime == startTime &&
                b.EndTime == endTime &&
                b.TotalPrice == 200m &&
                b.BookingOptions.Count == 0),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBooking_WithValidOptions_ShouldCalculatePriceAndCreateBooking()
    {
        var handler = CreateBookingHandler();

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var user = NewUser();
        var option = new Option("Projector", 50m);
        hall.AddOption(option);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 3m;
        var endTime = startTime.AddHours((double)durationHours);

        var command = new CreateBookingCommand(hallId, user.Id, startTime, durationHours, [option.Id]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _bookingRepository.HasOverlappingBookingAsync(
                hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(option.Id)),
                Arg.Any<CancellationToken>())
            .Returns([option]);
        _pricingService.CalculatePriceAsync(
                hall.BaseHourlyRate,
                Arg.Is<IReadOnlyCollection<decimal>?>(prices => prices != null && prices.Count == 1 && prices.First() == 50m),
                startTime,
                endTime,
                Arg.Any<CancellationToken>())
            .Returns(new PricingResult(300m, 50m, 350m));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SelectedOptions.Should().HaveCount(1);
        result.SelectedOptions.First().Name.Should().Be("Projector");
        result.SelectedOptions.First().Price.Should().Be(50m);
        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(50m);
        result.TotalCost.Should().Be(350m);

        await _bookingRepository.Received(1).AddAsync(
            Arg.Is<Booking>(b =>
                b.HallId == hall.Id &&
                b.BookingOptions.Count == 1 &&
                b.BookingOptions.First().OptionId == option.Id &&
                b.BookingOptions.First().PriceAtBooking == 50m),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetBookingByIdQueryHandler

    [Fact]
    public async Task GetBookingById_WhenBookedByCurrentUser_ShouldReturnBookingResponse()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _userRepository);

        var hall = new Hall("Conference Room A", 50, 100m);
        var owner = NewUser();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);
        var option = new Option("Projector", 50m);
        var booking = new Booking(hall, owner, startTime, endTime, 300m, 350m, [new BookingOption(option, 50m)]);

        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
            .Returns(booking);

        var result = await handler.Handle(
            new GetBookingByIdQuery(booking.Id, owner.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(booking.Id);
        result.HallId.Should().Be(hall.Id);
        result.HallName.Should().Be("Conference Room A");
        result.HallCapacity.Should().Be(50);
        result.HallBaseHourlyRate.Should().Be(100m);
        result.StartTime.Should().Be(startTime);
        result.EndTime.Should().Be(endTime);
        result.DurationHours.Should().Be(3m);
        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(50m);
        result.TotalCost.Should().Be(350m);

        await _userRepository.DidNotReceive().GetByIdAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBookingById_WhenBookingDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _userRepository);
        var bookingId = Guid.NewGuid();

        _bookingRepository.GetByIdAsync(bookingId, Arg.Any<CancellationToken>())
            .Returns((Booking?)null);

        var act = () => handler.Handle(
            new GetBookingByIdQuery(bookingId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Booking>>();
    }

    [Fact]
    public async Task GetBookingById_WhenBookedByAnotherUserAndRoleCustomer_ShouldThrowBookingAccessException()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _userRepository);

        var hall = new Hall("Conference Room A", 50, 100m);
        var owner = NewUser();
        var stranger = NewUser(UserRole.Customer);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new Booking(hall, owner, startTime, startTime.AddHours(2), 200m, 200m);

        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
            .Returns(booking);
        _userRepository.GetByIdAsync(stranger.Id, Arg.Any<CancellationToken>())
            .Returns(stranger);

        var act = () => handler.Handle(
            new GetBookingByIdQuery(booking.Id, stranger.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BookingAccessException>();
    }

    [Fact]
    public async Task GetBookingById_WhenBookedByAnotherUserAndRoleAdmin_ShouldReturnBookingResponse()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _userRepository);

        var hall = new Hall("Conference Room A", 50, 100m);
        var owner = NewUser();
        var admin = NewUser(UserRole.Admin);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new Booking(hall, owner, startTime, startTime.AddHours(2), 200m, 200m);

        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
            .Returns(booking);
        _userRepository.GetByIdAsync(admin.Id, Arg.Any<CancellationToken>())
            .Returns(admin);

        var result = await handler.Handle(
            new GetBookingByIdQuery(booking.Id, admin.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(booking.Id);
        result.TotalCost.Should().Be(200m);
    }

    [Fact]
    public async Task GetBookingById_WhenCurrentUserNotFoundInDb_ShouldThrowBookingAccessException()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _userRepository);

        var hall = new Hall("Conference Room A", 50, 100m);
        var owner = NewUser();
        var strangerId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new Booking(hall, owner, startTime, startTime.AddHours(2), 200m, 200m);

        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
            .Returns(booking);
        _userRepository.GetByIdAsync(strangerId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => handler.Handle(
            new GetBookingByIdQuery(booking.Id, strangerId), CancellationToken.None);

        await act.Should().ThrowAsync<BookingAccessException>();
    }

    #endregion
}

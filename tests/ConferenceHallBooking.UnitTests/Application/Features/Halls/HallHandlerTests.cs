using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Features.Halls.Handlers;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ConferenceHallBooking.UnitTests.Application.Features.Halls;

public class HallHandlerTests
{
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static Option NewOption(string name, decimal price) => new(name, price);

    #region CreateHallCommandHandler

    [Fact]
    public async Task CreateHall_WithoutOptions_ShouldCreateHallAndReturnResponse()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var command = new CreateHallCommand("Grand Hall", 100, 250m, []);

        _hallRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Grand Hall");
        result.Capacity.Should().Be(100);
        result.BaseHourlyRate.Should().Be(250m);
        result.Options.Should().BeEmpty();

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.Name == command.Name && h.HallOptions.Count == 0),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHall_WithExistingOptionIds_ShouldAddOptionsToHall()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var existingOption1 = NewOption("Projector", 50m);
        var existingOption2 = NewOption("Wi-Fi", 30m);
        var optionId1 = existingOption1.Id;
        var optionId2 = existingOption2.Id;

        var command = new CreateHallCommand("Grand Hall", 100, 250m, [optionId1, optionId2]);

        _hallRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId1) && ids.Contains(optionId2)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { existingOption1, existingOption2 });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().HaveCount(2);

        await _optionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId1) && ids.Contains(optionId2)),
            Arg.Any<CancellationToken>());
        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.HallOptions.Count == 2),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHall_WhenOptionIdsIsNull_ShouldCreateHallWithoutOptions()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var command = new CreateHallCommand("Grand Hall", 100, 250m, null);

        _hallRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().BeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHall_WhenOptionIdsDoNotExist_ShouldThrowOptionsNotFoundException()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var optionId = Guid.NewGuid();
        var command = new CreateHallCommand("Grand Hall", 100, 250m, [optionId]);

        _hallRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option>());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OptionsNotFoundException>();
        await _hallRepository.DidNotReceive().AddAsync(Arg.Any<Hall>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHall_WithDuplicateOptionIds_ShouldDeduplicateBeforeAdding()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var existingOption = NewOption("Projector", 50m);
        var optionId = existingOption.Id;
        var command = new CreateHallCommand("Grand Hall", 100, 250m, [optionId, optionId]);

        _hallRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(optionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { existingOption });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().HaveCount(1);

        await _optionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHall_WhenNameAlreadyExists_ShouldThrowHallNameAlreadyExistsException()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var command = new CreateHallCommand("Grand Hall", 100, 250m, []);

        _hallRepository.ExistsByNameAsync("Grand Hall", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallNameAlreadyExistsException>();
        await _hallRepository.DidNotReceive().AddAsync(Arg.Any<Hall>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateHallCommandHandler

    [Fact]
    public async Task UpdateHall_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var command = new UpdateHallCommand(hallId, "Updated Name", 150, 300m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenValid_ShouldUpdateHallPropertiesAndReturnResponse()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Old Name", 50, 100m);
        var command = new UpdateHallCommand(hallId, "New Name", 80, 150m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        existingHall.Name.Should().Be("New Name");
        existingHall.Capacity.Should().Be(80);
        existingHall.BaseHourlyRate.Should().Be(150m);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WithExistingOptionIds_ShouldAddThemToHall()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var existingOption = NewOption("Projector", 40m);
        var optionId = existingOption.Id;
        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, [optionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { existingOption });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().HaveCount(1);
        existingHall.HallOptions.Should().HaveCount(1);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenOptionIdsIsEmpty_ShouldRemoveAllExistingOptions()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        existingHall.AddOption(NewOption("A", 10m));
        existingHall.AddOption(NewOption("B", 20m));
        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _bookingRepository.GetBlockedOptionIdsForHallAsync(
                hallId,
                Arg.Any<IReadOnlyCollection<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().BeEmpty();
        existingHall.HallOptions.Should().BeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenReplacingSomeOptions_ShouldRemoveOldAndAddNew()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var keepOption = NewOption("Keep Option", 30m);
        var removeOption = NewOption("Remove Option", 35m);
        var addOption = NewOption("Add Option", 40m);

        existingHall.AddOption(keepOption);
        existingHall.AddOption(removeOption);

        var command = new UpdateHallCommand(
            hallId, "Updated Hall", 60, 120m, [keepOption.Id, addOption.Id]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(keepOption.Id) && ids.Contains(addOption.Id)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { keepOption, addOption });
        _bookingRepository.GetBlockedOptionIdsForHallAsync(
                hallId,
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(removeOption.Id)),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().HaveCount(2);
        existingHall.HallOptions.Select(ho => ho.OptionId)
            .Should().BeEquivalentTo([keepOption.Id, addOption.Id]);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenRemovingOptionUsedInUpcomingBookings_ShouldThrowHallOptionInUseException()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        typeof(Hall).GetProperty(nameof(Hall.Id))!.SetValue(existingHall, hallId);
        var keepOption = NewOption("Keep Option", 30m);
        var removeOption = NewOption("Remove Option", 35m);

        existingHall.AddOption(keepOption);
        existingHall.AddOption(removeOption);

        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, [keepOption.Id]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(keepOption.Id)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { keepOption });
        _bookingRepository.GetBlockedOptionIdsForHallAsync(
                hallId,
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(removeOption.Id)),
                Arg.Any<CancellationToken>())
            .Returns([removeOption.Id]);

        var act = () => handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HallOptionInUseException>();
        exception.Which.HallId.Should().Be(hallId);
        exception.Which.OptionIds.Should().Contain(removeOption.Id);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenOptionUsedOnlyInPastBookings_ShouldRemoveOptionSuccessfully()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        typeof(Hall).GetProperty(nameof(Hall.Id))!.SetValue(existingHall, hallId);
        var keepOption = NewOption("Keep Option", 30m);
        var removeOption = NewOption("Remove Option", 35m);

        existingHall.AddOption(keepOption);
        existingHall.AddOption(removeOption);

        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, [keepOption.Id]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(keepOption.Id)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { keepOption });
        _bookingRepository.GetBlockedOptionIdsForHallAsync(
                hallId,
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(removeOption.Id)),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        existingHall.HallOptions.Should().HaveCount(1);
        existingHall.HallOptions.First().OptionId.Should().Be(keepOption.Id);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenOptionIdsDoNotExist_ShouldThrowOptionsNotFoundException()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var nonexistentOptionId = Guid.NewGuid();
        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, [nonexistentOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(nonexistentOptionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option>());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OptionsNotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenNameUnchanged_ShouldNotCheckNameUniqueness()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Same Name", 50, 100m);
        var command = new UpdateHallCommand(hallId, "Same Name", 60, 120m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Same Name");
        existingHall.Capacity.Should().Be(60);

        await _hallRepository.DidNotReceive().ExistsByNameAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHall_WhenNewNameAlreadyExists_ShouldThrowHallNameAlreadyExistsException()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Old Name", 50, 100m);
        var command = new UpdateHallCommand(hallId, "Existing Hall", 50, 100m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _hallRepository.ExistsByNameAsync("Existing Hall", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallNameAlreadyExistsException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteHallCommandHandler

    [Fact]
    public async Task DeleteHall_WhenHallExists_ShouldDeleteAndSaveChanges()
    {
        var handler = new DeleteHallCommandHandler(_hallRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("To Delete", 20, 50m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _bookingRepository.GetBookingCountByHallIdAsync(existingHall.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        await handler.Handle(new DeleteHallCommand(hallId), CancellationToken.None);

        _hallRepository.Received(1).Delete(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteHall_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new DeleteHallCommandHandler(_hallRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(new DeleteHallCommand(hallId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();
        _hallRepository.DidNotReceive().Delete(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteHall_WhenHallHasBookings_ShouldThrowHallHasBookingsException()
    {
        var handler = new DeleteHallCommandHandler(_hallRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Has Bookings", 20, 50m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _bookingRepository.GetBookingCountByHallIdAsync(existingHall.Id, Arg.Any<CancellationToken>())
            .Returns(3);

        var act = () => handler.Handle(new DeleteHallCommand(hallId), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HallHasBookingsException>();
        exception.Which.HallId.Should().Be(existingHall.Id);
        exception.Which.BookingCount.Should().Be(3);

        _hallRepository.DidNotReceive().Delete(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetHallByIdQueryHandler

    [Fact]
    public async Task GetHallById_WhenHallExists_ShouldReturnHallResponse()
    {
        var handler = new GetHallByIdQueryHandler(_hallRepository);
        var hallId = Guid.NewGuid();
        var hall = new Hall("Grand Hall", 100, 250m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        var result = await handler.Handle(new GetHallByIdQuery(hallId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(hall.Id);
        result.Name.Should().Be("Grand Hall");
        result.Capacity.Should().Be(100);
        result.BaseHourlyRate.Should().Be(250m);
        result.Options.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHallById_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new GetHallByIdQueryHandler(_hallRepository);
        var hallId = Guid.NewGuid();

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(new GetHallByIdQuery(hallId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();
    }

    [Fact]
    public async Task GetHallById_WhenHallHasOptions_ShouldMapOptionsCorrectly()
    {
        var handler = new GetHallByIdQueryHandler(_hallRepository);
        var hallId = Guid.NewGuid();
        var hall = new Hall("Grand Hall", 100, 250m);
        var option = new Option("Projector", 50m);
        hall.AddOption(option);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        var result = await handler.Handle(new GetHallByIdQuery(hallId), CancellationToken.None);

        result.Options.Should().HaveCount(1);
        var opt = result.Options.First();
        opt.Id.Should().Be(option.Id);
        opt.Name.Should().Be("Projector");
        opt.Price.Should().Be(50m);
    }

    #endregion

    #region SearchAvailableHallsQueryHandler

    [Fact]
    public async Task SearchAvailableHalls_WhenHallsExist_ShouldReturnMappedResponses()
    {
        var handler = new SearchAvailableHallsQueryHandler(_hallRepository);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var query = new SearchAvailableHallsQuery(start, end, 50);
        var hall = new Hall("Available Hall", 100, 200m);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns([hall]);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        var response = result.First();
        response.Name.Should().Be("Available Hall");
        response.Capacity.Should().Be(100);
        response.BaseHourlyRate.Should().Be(200m);
        response.Options.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAvailableHalls_WhenNoHallsAvailable_ShouldReturnEmptyCollection()
    {
        var handler = new SearchAvailableHallsQueryHandler(_hallRepository);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var query = new SearchAvailableHallsQuery(start, end, 50);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    #endregion
}

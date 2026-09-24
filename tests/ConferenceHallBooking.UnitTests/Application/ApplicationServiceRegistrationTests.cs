using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Application.Services.Bookings;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceHallBooking.UnitTests.Application;

public class ApplicationServiceRegistrationTests
{
    [Fact]
    public void AddApplication_ShouldRegisterMediatrPipelineBehavior()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var provider = services.BuildServiceProvider();
        var behaviors = provider.GetServices<IPipelineBehavior<TestRequest, TestResponse>>().ToList();

        behaviors.Should().ContainSingle();
        behaviors[0].GetType().Name.Should().Be("ValidationBehavior`2");
    }

    [Fact]
    public void AddApplication_ShouldRegisterHallValidators()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var provider = services.BuildServiceProvider();
        var createValidator = provider.GetService<IValidator<CreateHallCommand>>();
        var updateValidator = provider.GetService<IValidator<UpdateHallCommand>>();
        var searchValidator = provider.GetService<IValidator<SearchAvailableHallsQuery>>();

        createValidator.Should().NotBeNull();
        updateValidator.Should().NotBeNull();
        searchValidator.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ShouldRegisterPricingServiceAsScoped()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = services.Single(d => d.ServiceType == typeof(IPricingService));

        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().Be(typeof(PricingService));
    }

    private sealed record TestRequest(string Name) : IRequest<TestResponse>;

    private sealed record TestResponse(string Value);
}

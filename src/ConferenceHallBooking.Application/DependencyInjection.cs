using ConferenceHallBooking.Application.Behaviours;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Application.Services.Bookings;
using FluentValidation;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers MediatR, the validation pipeline and application services.</summary>
public static class ApplicationServiceExtensions
{
    /// <summary>Adds the Application layer to the service collection.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        services.AddScoped<IPricingService, PricingService>();

        return services;
    }
}

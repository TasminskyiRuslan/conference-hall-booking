using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Interfaces;
using ConferenceHallBooking.Application.Interfaces.Auth;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using ConferenceHallBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<IOptionRepository, OptionRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<IAuthService, AuthService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        return services;
    }
}

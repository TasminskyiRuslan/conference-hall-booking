using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}
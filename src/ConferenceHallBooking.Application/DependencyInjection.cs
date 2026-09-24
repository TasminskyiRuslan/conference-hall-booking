using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
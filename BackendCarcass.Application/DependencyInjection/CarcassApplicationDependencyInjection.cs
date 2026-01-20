using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BackendCarcass.Application.DependencyInjection;

public static class CarcassApplicationDependencyInjection
{
    public static IServiceCollection AddScopedAllCarcassApplicationServices(this IServiceCollection services,
        ILogger logger, bool debugMode)
    {
        if (debugMode)
        {
            logger.Information("{MethodName} Started", nameof(AddScopedAllCarcassApplicationServices));
        }

        var assembly = typeof(IScopeServiceCarcassApplication).Assembly;
        foreach (var type in assembly.ExportedTypes.Where(x =>
                     typeof(IScopeServiceCarcassApplication).IsAssignableFrom(x) &&
                     x is { IsInterface: false, IsAbstract: false }))
        {
            services.AddScoped(type);
        }

        if (debugMode)
        {
            logger.Information("{MethodName} Finished", nameof(AddScopedAllCarcassApplicationServices));
        }

        return services;
    }
}

using System;
using System.Linq;
using System.Reflection;
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

        Assembly assembly = typeof(IScopeServiceCarcassApplication).Assembly;
        foreach (Type type in assembly.ExportedTypes.Where(x =>
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

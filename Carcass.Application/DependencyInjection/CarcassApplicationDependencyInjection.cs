using Microsoft.Extensions.DependencyInjection;

namespace Carcass.Application.DependencyInjection;

public static class CarcassApplicationDependencyInjection
{

    public static IServiceCollection AddScopedAllCarcassApplicationServices(this IServiceCollection services, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{nameof(AddScopedAllCarcassApplicationServices)} Started");

        var assembly = typeof(IScopeServiceCarcassApplication).Assembly;
        foreach (var type in assembly.ExportedTypes.Where(x =>
                     typeof(IScopeServiceCarcassApplication).IsAssignableFrom(x) &&
                     x is { IsInterface: false, IsAbstract: false }))
            services.AddScoped(type);

        if (debugMode)
            Console.WriteLine($"{nameof(AddScopedAllCarcassApplicationServices)} Finished");

        return services;
    }

}
using System;
using Microsoft.Extensions.DependencyInjection;

namespace CarcassDom.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassDomDependencyInjection
{
    public static IServiceCollection AddCarcassDom(this IServiceCollection services, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{nameof(AddCarcassDom)} Started");

        services.AddSingleton<FilterSortManager>();

        if (debugMode)
            Console.WriteLine($"{nameof(AddCarcassDom)} Finished");

        return services;
    }
}
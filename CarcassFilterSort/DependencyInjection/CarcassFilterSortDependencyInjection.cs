using System;
using Microsoft.Extensions.DependencyInjection;

namespace CarcassFilterSort.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassFilterSortDependencyInjection
{
    public static IServiceCollection AddCarcassFilterSort(this IServiceCollection services, bool debugMode)
    {
        //if (debugMode)
        //{
        //    Console.WriteLine($"{nameof(CarcassFilterSortDependencyInjection)} Started");
        //}

        services.AddSingleton<FilterSortManager>();

        //if (debugMode)
        //    Console.WriteLine($"{nameof(CarcassFilterSortDependencyInjection)} Finished");

        return services;
    }
}

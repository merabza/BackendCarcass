using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BackendCarcass.FilterSort.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassFilterSortDependencyInjection
{
    public static IServiceCollection AddCarcassFilterSort(this IServiceCollection services, ILogger logger,
        bool debugMode)
    {
        if (debugMode) logger.Information("{MethodName} Started", nameof(AddCarcassFilterSort));

        services.AddSingleton<FilterSortManager>();

        if (debugMode) logger.Information("{MethodName} Finished", nameof(AddCarcassFilterSort));

        return services;
    }
}
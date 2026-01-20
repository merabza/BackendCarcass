using BackendCarcass.Identity;
using BackendCarcass.MasterData;
using BackendCarcass.Rights;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BackendCarcass.Repositories.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassRepositoriesDependencyInjection
{
    public static IServiceCollection AddCarcassRepositories(this IServiceCollection services, ILogger logger,
        bool debugMode)
    {
        if (debugMode) logger.Information("{MethodName} Started", nameof(AddCarcassRepositories));

        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IMenuRightsRepository, MenuRightsRepository>();
        services.AddScoped<IDataTypesRepository, DataTypesRepository>();
        services.AddScoped<IReturnValuesRepository, SqlReturnValuesRepository>();
        services.AddScoped<IRightsRepository, RightsRepository>();
        services.AddScoped<IUserClaimsRepository, UserClaimsRepository>();

        if (debugMode) logger.Information("{MethodName} Finished", nameof(AddCarcassRepositories));

        return services;
    }
}
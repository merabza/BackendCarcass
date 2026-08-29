using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData;
using BackendCarcass.Application.Rights;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BackendCarcass.Application.Repositories.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassRepositoriesDependencyInjection
{
    public static IServiceCollection AddCarcassRepositories(this IServiceCollection services, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(AddCarcassRepositories));

        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IMenuRightsRepository, MenuRightsRepository>();
        services.AddScoped<IDataTypesRepository, DataTypesRepository>();
        services.AddScoped<IReturnValuesRepository, SqlReturnValuesRepository>();
        services.AddScoped<IRightsRepository, RightsRepository>();
        services.AddScoped<IUserClaimsRepository, UserClaimsRepository>();

        debugLogger?.Information("{MethodName} Finished", nameof(AddCarcassRepositories));

        return services;
    }
}

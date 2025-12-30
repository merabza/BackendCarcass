using System;
using CarcassApplication.Repositories;
using CarcassDom;
using CarcassIdentity;
using CarcassMasterDataDom;
using Microsoft.Extensions.DependencyInjection;

namespace CarcassRepositories.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassRepositoriesDependencyInjection
{
    public static IServiceCollection AddCarcassRepositories(this IServiceCollection services, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{nameof(AddCarcassRepositories)} Started");

        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IMenuRightsRepository, MenuRightsRepository>();
        services.AddScoped<IDataTypesRepository, DataTypesRepository>();
        services.AddScoped<IReturnValuesRepository, SqlReturnValuesRepository>();
        services.AddScoped<IRightsRepository, RightsRepository>();
        services.AddScoped<IUserClaimsRepository, UserClaimsRepository>();

        if (debugMode)
            Console.WriteLine($"{nameof(AddCarcassRepositories)} Finished");

        return services;
    }
}
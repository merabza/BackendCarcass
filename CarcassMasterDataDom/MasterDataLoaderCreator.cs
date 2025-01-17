using System;
using System.Collections.Generic;
using CarcassMasterDataDom.Crud;
using CarcassMasterDataDom.Models;
using LibCrud;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom;

public class MasterDataLoaderCreator : IMasterDataLoaderCreator
{
    private readonly ILogger<MasterDataLoaderCreator> _logger;
    protected readonly IServiceProvider Services;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataLoaderCreator(ILogger<MasterDataLoaderCreator> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;
    }

    public virtual OneOf<IMasterDataLoader, IEnumerable<Err>> CreateMasterDataLoader(string queryName)
    {
        // ReSharper disable once using
        var scope = Services.CreateScope();

        return MasterDataCrud
            .Create(queryName, _logger, scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>())
            .Match<OneOf<IMasterDataLoader, IEnumerable<Err>>>(f0 => f0, f1 => (Err[])f1);
    }

    public virtual OneOf<CrudBase, IEnumerable<Err>> CreateMasterDataCrud(string tableName)
    {
        // ReSharper disable once using
        var scope = Services.CreateScope();
        var carcassMasterDataRepository = scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>();

        return tableName switch
        {
            "users" => new UsersCrud(_logger, scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
                carcassMasterDataRepository),
            "roles" => new RolesCrud(_logger, scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>(),
                carcassMasterDataRepository),
            _ => MasterDataCrud.Create(tableName, _logger, carcassMasterDataRepository)
                .Match<OneOf<CrudBase, IEnumerable<Err>>>(f0 => f0, f1 => (Err[])f1)
        };
    }
}
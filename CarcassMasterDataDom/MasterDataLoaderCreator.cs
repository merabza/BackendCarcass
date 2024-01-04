using System;
using CarcassMasterDataDom.Crud;
using CarcassMasterDataDom.Models;
using LibCrud;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarcassMasterDataDom;

public class MasterDataLoaderCreator : IMasterDataLoaderCreator
{
    private readonly ILogger<MasterDataLoaderCreator> _logger;
    protected readonly IServiceProvider Services;

    protected MasterDataLoaderCreator(ILogger<MasterDataLoaderCreator> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;
    }

    public virtual IMasterDataLoader CreateMasterDataLoader(string queryName)
    {
        var scope = Services.CreateScope();

        //return queryName switch
        //{
        //    "menuToCrudTypes" => new MenuToCrudTypesMdLoader(scope.ServiceProvider
        //        .GetRequiredService<IDataTypesRepository>()),
        //    "dataTypesToDataTypes" => new DataTypesToDataTypesMdLoader(scope.ServiceProvider
        //        .GetRequiredService<IDataTypesRepository>()),
        //    "dataTypesToCrudTypes" => new DataTypesToCrudTypesMdLoader(scope.ServiceProvider
        //        .GetRequiredService<IDataTypesRepository>()),
        //    _ => new MasterDataCrud(queryName, _logger,
        //        scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>())
        //};

        return new MasterDataCrud(queryName, _logger,
            scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>());
    }

    public virtual CrudBase CreateMasterDataCrud(string tableName)
    {
        var scope = Services.CreateScope();
        var carcassMasterDataRepository = scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>();

        return tableName switch
        {
            "users" => new UsersCrud(_logger, scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
                carcassMasterDataRepository),
            "roles" => new RolesCrud(_logger, scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>(),
                carcassMasterDataRepository),
            _ => new MasterDataCrud(tableName, _logger, carcassMasterDataRepository)
        };
    }
}
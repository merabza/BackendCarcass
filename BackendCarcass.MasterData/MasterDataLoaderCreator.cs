using System;
using System.Linq;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData.Crud;
using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.DomainShared.Repositories;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public /*open*/ class MasterDataLoaderCreator : IMasterDataLoaderCreator
{
    private readonly ILogger<MasterDataLoaderCreator> _logger;
    protected readonly IServiceProvider Services;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataLoaderCreator(ILogger<MasterDataLoaderCreator> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;
    }

    public virtual OneOf<IMasterDataLoader, Err[]> CreateMasterDataLoader(string queryName)
    {
        // ReSharper disable once using
#pragma warning disable CA2000
        var scope = Services.CreateScope();
#pragma warning restore CA2000
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        return MasterDataCrud
            .Create(queryName, _logger, scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>(),
                unitOfWork).Match<OneOf<IMasterDataLoader, Err[]>>(f0 => f0, f1 => f1.ToArray());
    }

    public virtual OneOf<CrudBase, Err[]> CreateMasterDataCrud(string tableName)
    {
        // ReSharper disable once using
#pragma warning disable CA2000
        var scope = Services.CreateScope();
#pragma warning restore CA2000
        var carcassMasterDataRepository = scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        return tableName switch
        {
            "users" => new UsersCrud(_logger, scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
                unitOfWork),
            "roles" => new RolesCrud(_logger, scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>(),
                unitOfWork),
            _ => MasterDataCrud.Create(tableName, _logger, carcassMasterDataRepository, unitOfWork)
                .Match<OneOf<CrudBase, Err[]>>(f0 => f0, f1 => f1.ToArray())
        };
    }
}

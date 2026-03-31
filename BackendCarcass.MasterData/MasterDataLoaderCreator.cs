using System;
using System.Linq;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData.Crud;
using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.Domain.Abstractions;
using SystemTools.SystemToolsShared;
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

    public virtual OneOf<IMasterDataLoader, Error[]> CreateMasterDataLoader(string queryName)
    {
        // ReSharper disable once using
#pragma warning disable CA2000
        IServiceScope scope = Services.CreateScope();
#pragma warning restore CA2000
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var databaseAbstraction = scope.ServiceProvider.GetRequiredService<IDatabaseAbstraction>();

        return MasterDataCrud
            .Create(queryName, _logger, scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>(),
                unitOfWork, databaseAbstraction).Match<OneOf<IMasterDataLoader, Error[]>>(f0 => f0, f1 => f1.ToArray());
    }

    public virtual OneOf<CrudBase, Error[]> CreateMasterDataCrud(string tableName)
    {
        // ReSharper disable once using
#pragma warning disable CA2000
        IServiceScope scope = Services.CreateScope();
#pragma warning restore CA2000
        var carcassMasterDataRepository = scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var databaseAbstraction = scope.ServiceProvider.GetRequiredService<IDatabaseAbstraction>();

        return tableName switch
        {
            "users" => new UsersCrud(_logger, scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
                unitOfWork, databaseAbstraction),
            "roles" => new RolesCrud(_logger, scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>(),
                unitOfWork, databaseAbstraction),
            _ => MasterDataCrud.Create(tableName, _logger, carcassMasterDataRepository, unitOfWork, databaseAbstraction)
                .Match<OneOf<CrudBase, Error[]>>(f0 => f0, f1 => f1.ToArray())
        };
    }
}

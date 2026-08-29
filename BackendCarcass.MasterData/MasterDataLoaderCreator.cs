using System;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData.Crud;
using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.MasterData;

public /*open*/ class MasterDataLoaderCreator : IMasterDataLoaderCreator
{
    protected readonly IServiceProvider Services;
    private readonly ILogger<MasterDataLoaderCreator> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataLoaderCreator(ILogger<MasterDataLoaderCreator> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;
    }

    public virtual Result<IMasterDataLoader> CreateMasterDataLoader(string queryName)
    {
        // ReSharper disable once using
#pragma warning disable CA2000
        IServiceScope scope = Services.CreateScope();
#pragma warning restore CA2000
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var databaseAbstraction = scope.ServiceProvider.GetRequiredService<IDatabaseAbstraction>();

        Result<MasterDataCrud> createResult = MasterDataCrud.Create(queryName, _logger,
            scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>(), unitOfWork, databaseAbstraction);
        return createResult.IsFailure ? Result.Failure<IMasterDataLoader>(createResult.Error) : createResult.Value;
    }

    public virtual Result<CrudBase> CreateMasterDataCrud(string tableName)
    {
        // ReSharper disable once using
#pragma warning disable CA2000
        IServiceScope scope = Services.CreateScope();
#pragma warning restore CA2000
        var carcassMasterDataRepository = scope.ServiceProvider.GetRequiredService<ICarcassMasterDataRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var databaseAbstraction = scope.ServiceProvider.GetRequiredService<IDatabaseAbstraction>();

        switch (tableName)
        {
            case "users":
                return new UsersCrud(_logger, scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
                    unitOfWork, databaseAbstraction);
            case "roles":
                return new RolesCrud(_logger, scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>(),
                    unitOfWork, databaseAbstraction);
            default:
                Result<MasterDataCrud> createResult = MasterDataCrud.Create(tableName, _logger,
                    carcassMasterDataRepository, unitOfWork, databaseAbstraction);
                return createResult.IsFailure ? Result.Failure<CrudBase>(createResult.Error) : createResult.Value;
        }
    }
}

using System;
using BackendCarcass.Application.Crud;
using BackendCarcass.Application.MasterData.Crud;
using BackendCarcass.Application.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Application.MasterData;

public /*open*/ class MasterDataLoaderCreator : IMasterDataLoaderCreator
{
    private readonly ILogger<MasterDataLoaderCreator> _logger;
    private readonly IServiceProvider _services;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataLoaderCreator(ILogger<MasterDataLoaderCreator> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    public virtual Result<IMasterDataLoader> CreateMasterDataLoader(string queryName)
    {
        // ReSharper disable once using
#pragma warning disable CA2000
        IServiceScope scope = _services.CreateScope();
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
        IServiceScope scope = _services.CreateScope();
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

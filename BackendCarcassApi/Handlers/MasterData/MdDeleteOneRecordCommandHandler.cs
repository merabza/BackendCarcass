﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.MasterData;
using BackendCarcassContracts.Errors;
using CarcassMasterDataDom;
using MediatR;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdDeleteOneRecordCommandHandler : ICommandHandler<MdDeleteOneRecordCommandRequest>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdDeleteOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(MdDeleteOneRecordCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return createMasterDataCrudResult.AsT1;
        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.Match<OneOf<Unit, Err[]>>(y =>
        {
            var errors = y.ToList();
            errors.Add(MasterDataApiErrors.CannotDeleteNewRecord);
            return errors.ToArray();
        }, () => new Unit());
    }
}
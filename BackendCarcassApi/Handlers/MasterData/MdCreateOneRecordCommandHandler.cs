using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.MasterData;
using BackendCarcassContracts.Errors;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    MdCreateOneRecordCommandHandler : ICommandHandler<MdCreateOneRecordRequestCommand, MasterDataCrudLoadedData>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdCreateOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<MasterDataCrudLoadedData, Err[]>> Handle(MdCreateOneRecordRequestCommand request,
        CancellationToken cancellationToken = default)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        // ReSharper disable once using
        // ReSharper disable once DisposableConstructor
        using var reader = new StreamReader(request.HttpRequest.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var crudData = new MasterDataCrudData(body);
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return createMasterDataCrudResult.AsT1;
        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.Create(crudData, cancellationToken);
        return result.Match<OneOf<MasterDataCrudLoadedData, Err[]>>(rcd => (MasterDataCrudLoadedData)rcd,
            y => Err.RecreateErrors(y, MasterDataApiErrors.CannotCreateNewRecord));
    }
}
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.BackendCarcassContracts.Errors;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.CreateOneRecord;

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
        CancellationToken cancellationToken)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        // ReSharper disable once using
        // ReSharper disable once DisposableConstructor
        using var reader = new StreamReader(request.HttpRequest.Body);
        string body = await reader.ReadToEndAsync(cancellationToken);

        var crudData = new MasterDataCrudData(body);
        OneOf<CrudBase, Err[]> createMasterDataCrudResult =
            _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        CrudBase? masterDataCruder = createMasterDataCrudResult.AsT0;
        OneOf<ICrudData, Err[]> result = await masterDataCruder.Create(crudData, cancellationToken);
        return result.Match<OneOf<MasterDataCrudLoadedData, Err[]>>(rcd => (MasterDataCrudLoadedData)rcd,
            y => Err.RecreateErrors(y, MasterDataApiErrors.CannotCreateNewRecord));
    }
}

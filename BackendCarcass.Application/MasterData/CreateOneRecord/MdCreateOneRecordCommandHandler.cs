using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Crud;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcassShared.Contracts.Errors;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

//using CrudBase = BackendCarcass.Application.Crud.CrudBase;
//using ICrudData = BackendCarcass.Application.Crud.ICrudData;
//using MasterDataCrudData = BackendCarcass.Application.MasterData.Models.MasterDataCrudData;
//using MasterDataCrudLoadedData = BackendCarcass.Application.MasterData.Models.MasterDataCrudLoadedData;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.CreateOneRecord;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdCreateOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    : ICommandHandler<MdCreateOneRecordRequestCommand, MasterDataCrudLoadedData>
{
    public async Task<Result<MasterDataCrudLoadedData>> Handle(MdCreateOneRecordRequestCommand request,
        CancellationToken cancellationToken)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        // ReSharper disable once using
        // ReSharper disable once DisposableConstructor
        using var reader = new StreamReader(request.HttpRequest.Body);
        string body = await reader.ReadToEndAsync(cancellationToken);

        var crudData = new MasterDataCrudData(body);
        Result<CrudBase> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsFailure)
        {
            return Result.Failure<MasterDataCrudLoadedData>(createMasterDataCrudResult.Error);
        }

        CrudBase masterDataCruder = createMasterDataCrudResult.Value;
        Result<ICrudData> result = await masterDataCruder.Create(crudData, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<MasterDataCrudLoadedData>(ErrorOmd
                .RecreateErrors(result.Error.ToErrorOmdArray(), MasterDataApiErrors.CannotCreateNewRecord).ToError());
        }

        return (MasterDataCrudLoadedData)result.Value;
    }
}

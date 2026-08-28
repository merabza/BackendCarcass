using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.UpdateOneRecord;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdUpdateOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    : ICommandHandler<MdUpdateOneRecordRequestCommand>
{
    public async Task<Result> Handle(MdUpdateOneRecordRequestCommand request, CancellationToken cancellationToken)
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
            return Result.Failure(createMasterDataCrudResult.Error);
        }

        CrudBase masterDataCruder = createMasterDataCrudResult.Value;
        Option<ErrorOmd[]> result = await masterDataCruder.Update(request.Id, crudData, cancellationToken);
        return result.Match<Result>(
            y => Result.Failure(ErrorOmd.RecreateErrors(y, MasterDataApiErrors.CannotUpdateNewRecord).ToError()),
            () => Result.Success());
    }
}

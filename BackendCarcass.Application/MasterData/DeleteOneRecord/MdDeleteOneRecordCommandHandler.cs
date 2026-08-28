using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.DeleteOneRecord;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdDeleteOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    : ICommandHandler<MdDeleteOneRecordRequestCommand>
{
    public async Task<Result> Handle(MdDeleteOneRecordRequestCommand request, CancellationToken cancellationToken)
    {
        Result<CrudBase> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsFailure)
        {
            return Result.Failure(createMasterDataCrudResult.Error);
        }

        CrudBase masterDataCruder = createMasterDataCrudResult.Value;
        Option<ErrorOmd[]> result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.Match<Result>(
            y => Result.Failure(ErrorOmd.RecreateErrors(y, MasterDataApiErrors.CannotDeleteNewRecord).ToError()),
            () => Result.Success());
    }
}

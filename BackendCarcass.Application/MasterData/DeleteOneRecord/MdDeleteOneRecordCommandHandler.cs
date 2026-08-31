using System.Threading;
using System.Threading.Tasks;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using CrudBase = BackendCarcass.Application.Crud.CrudBase;

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
        Result result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }
}

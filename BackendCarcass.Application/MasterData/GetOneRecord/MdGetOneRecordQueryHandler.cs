using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Crud;
using BackendCarcass.Application.MasterData.Models;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.GetOneRecord;

public sealed class MdGetOneRecordQueryHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    : IQueryHandler<MdGetOneRecordRequestQuery, MasterDataCrudLoadedData>
{
    public async Task<Result<MasterDataCrudLoadedData>> Handle(MdGetOneRecordRequestQuery request,
        CancellationToken cancellationToken)
    {
        Result<CrudBase> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsFailure)
        {
            return Result.Failure<MasterDataCrudLoadedData>(createMasterDataCrudResult.Error);
        }

        CrudBase masterDataCruder = createMasterDataCrudResult.Value;
        Result<ICrudData> result = await masterDataCruder.GetOne(request.Id, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<MasterDataCrudLoadedData>(result.Error);
        }

        return (MasterDataCrudLoadedData)result.Value;
    }
}

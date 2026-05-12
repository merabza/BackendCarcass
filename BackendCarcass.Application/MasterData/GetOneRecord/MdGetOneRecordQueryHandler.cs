using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;


namespace BackendCarcass.Application.MasterData.GetOneRecord;

public sealed class MdGetOneRecordQueryHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator) : IQueryHandler<MdGetOneRecordRequestQuery, MasterDataCrudLoadedData>
{

    public async Task<OneOf<MasterDataCrudLoadedData, Error[]>> Handle(MdGetOneRecordRequestQuery request,
        CancellationToken cancellationToken)
    {
        OneOf<CrudBase, Error[]> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1.ToArray();
        }

        CrudBase? masterDataCruder = createMasterDataCrudResult.AsT0;
        OneOf<ICrudData, Error[]> result = await masterDataCruder.GetOne(request.Id, cancellationToken);
        return result.Match<OneOf<MasterDataCrudLoadedData, Error[]>>(r => (MasterDataCrudLoadedData)r,
            e => e.ToArray());
    }
}

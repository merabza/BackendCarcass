using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.MasterData;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdGetOneRecordQueryHandler : IQueryHandler<MdGetOneRecordRequestQuery, MasterDataCrudLoadedData>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdGetOneRecordQueryHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<MasterDataCrudLoadedData, Err[]>> Handle(MdGetOneRecordRequestQuery request,
        CancellationToken cancellationToken = default)
    {
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return createMasterDataCrudResult.AsT1.ToArray();
        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.GetOne(request.Id, cancellationToken);
        return result.Match<OneOf<MasterDataCrudLoadedData, Err[]>>(r => (MasterDataCrudLoadedData)r, e => e.ToArray());
    }
}
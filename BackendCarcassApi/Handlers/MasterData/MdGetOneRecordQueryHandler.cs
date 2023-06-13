using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.MasterData;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdGetOneRecordQueryHandler : IQueryHandler<MdGetOneRecordQueryRequest, MasterDataCrudLoadedData>
{
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;

    public MdGetOneRecordQueryHandler(IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<MasterDataCrudLoadedData, IEnumerable<Err>>> Handle(
        MdGetOneRecordQueryRequest request, CancellationToken cancellationToken)
    {
        var masterDataCruder = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        var result = await masterDataCruder.GetOne(request.Id);
        return result.Match<OneOf<MasterDataCrudLoadedData, IEnumerable<Err>>>(r => (MasterDataCrudLoadedData)r,
            e => e);
    }
}
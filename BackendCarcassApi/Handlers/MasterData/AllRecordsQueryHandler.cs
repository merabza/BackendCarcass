using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.MasterData;
using BackendCarcassApi.QueryResponses;
using CarcassMasterDataDom;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    AllRecordsQueryHandler : IQueryHandler<MdGetTableAllRecordsQueryRequest, MdGetTableAllRecordsQueryResponse>
{
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;


    public AllRecordsQueryHandler(IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<MdGetTableAllRecordsQueryResponse, IEnumerable<Err>>> Handle(
        MdGetTableAllRecordsQueryRequest request, CancellationToken cancellationToken)
    {
        var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(request.TableName);
        var entResult = await loader.GetAllRecords();
        return entResult.Match<OneOf<MdGetTableAllRecordsQueryResponse, IEnumerable<Err>>>(
            r => new MdGetTableAllRecordsQueryResponse(r), e => e);
    }
}
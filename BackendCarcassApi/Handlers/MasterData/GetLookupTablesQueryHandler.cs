using System.Collections.Generic;
using System.Linq;
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
public sealed class GetLookupTablesQueryHandler
    (IReturnValuesRepository rvRepo, IReturnValuesLoaderCreator returnValuesLoaderCreator) : IQueryHandler<
        MdGetLookupTablesQueryRequest, MdGetLookupTablesQueryResponse>
{
    public async Task<OneOf<MdGetLookupTablesQueryResponse, IEnumerable<Err>>> Handle(
        MdGetLookupTablesQueryRequest request, CancellationToken cancellationToken)
    {
        var reqQuery = request.HttpRequest.Query["tables"];
        List<string> tableNames = reqQuery.Where(tableName => tableName is not null).Distinct().ToList()!;
        var mdLoader = new ReturnValuesLoader(tableNames, rvRepo, returnValuesLoaderCreator);
        var loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetLookupTablesQueryResponse, IEnumerable<Err>>>(
            r => new MdGetLookupTablesQueryResponse(r), e => (Err[])e);
    }
}
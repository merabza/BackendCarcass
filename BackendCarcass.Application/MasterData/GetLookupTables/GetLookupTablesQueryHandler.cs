using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.MasterData.GetLookupTables;

public sealed class GetLookupTablesQueryHandler(
    IReturnValuesRepository rvRepo,
    IReturnValuesLoaderCreator returnValuesLoaderCreator)
    : IQueryHandler<MdGetLookupTablesRequestQuery, MdGetLookupTablesQueryResponse>
{
    public async Task<OneOf<MdGetLookupTablesQueryResponse, Error[]>> Handle(MdGetLookupTablesRequestQuery request,
        CancellationToken cancellationToken)
    {
        //var reqQuery = request.HttpRequest.Query["tables"];
        List<string?> tableNames =
            [.. request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()]!;
        var mdLoader = new ReturnValuesLoader(tableNames, rvRepo, returnValuesLoaderCreator);
        OneOf<Dictionary<string, IEnumerable<SrvModel>>, Error[]> loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetLookupTablesQueryResponse, Error[]>>(
            r => new MdGetLookupTablesQueryResponse(r), e => e.ToArray());
    }
}

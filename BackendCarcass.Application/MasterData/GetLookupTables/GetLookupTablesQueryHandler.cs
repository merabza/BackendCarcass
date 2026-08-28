using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.GetLookupTables;

public sealed class GetLookupTablesQueryHandler(
    IReturnValuesRepository rvRepo,
    IReturnValuesLoaderCreator returnValuesLoaderCreator)
    : IQueryHandler<MdGetLookupTablesRequestQuery, MdGetLookupTablesQueryResponse>
{
    public async Task<Result<MdGetLookupTablesQueryResponse>> Handle(MdGetLookupTablesRequestQuery request,
        CancellationToken cancellationToken)
    {
        //var reqQuery = request.HttpRequest.Query["tables"];
        List<string?> tableNames =
            [.. request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()];
        var mdLoader = new ReturnValuesLoader(tableNames, rvRepo, returnValuesLoaderCreator);
        Result<Dictionary<string, IEnumerable<SrvModel>>> loaderResult = await mdLoader.Run(cancellationToken);
        if (loaderResult.IsFailure)
        {
            return Result.Failure<MdGetLookupTablesQueryResponse>(loaderResult.Error);
        }

        return new MdGetLookupTablesQueryResponse(loaderResult.Value);
    }
}

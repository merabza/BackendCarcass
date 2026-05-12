using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.MasterData.GetMultipleTablesRows;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTablesQueryHandler(IMasterDataLoaderCreator masterDataLoaderCreator)
    : IQueryHandler<MdGetTablesRequestQuery, MdGetTablesQueryResponse>
{
    public async Task<OneOf<MdGetTablesQueryResponse, Error[]>> Handle(MdGetTablesRequestQuery request,
        CancellationToken cancellationToken)
    {
        List<string> tableNames = request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()
            .ToList()!;
        var mdLoader = new MasterDataLoader(tableNames, masterDataLoaderCreator);
        OneOf<Dictionary<string, IEnumerable<dynamic>>, Error[]> loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetTablesQueryResponse, Error[]>>(r => new MdGetTablesQueryResponse(r),
            e => e.ToArray());
    }
}

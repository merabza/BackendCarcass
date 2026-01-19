using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterData;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.MasterData.GetMultipleTablesRows;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTablesQueryHandler : IQueryHandler<MdGetTablesRequestQuery, MdGetTablesQueryResponse>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCreator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetTablesQueryHandler(IMasterDataLoaderCreator masterDataLoaderCreator)
    {
        _masterDataLoaderCreator = masterDataLoaderCreator;
    }

    public async Task<OneOf<MdGetTablesQueryResponse, Err[]>> Handle(MdGetTablesRequestQuery request,
        CancellationToken cancellationToken)
    {
        List<string> tableNames = request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()
            .ToList()!;
        var mdLoader = new MasterDataLoader(tableNames, _masterDataLoaderCreator);
        OneOf<Dictionary<string, IEnumerable<dynamic>>, Err[]> loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetTablesQueryResponse, Err[]>>(r => new MdGetTablesQueryResponse(r),
            e => e.ToArray());
    }
}

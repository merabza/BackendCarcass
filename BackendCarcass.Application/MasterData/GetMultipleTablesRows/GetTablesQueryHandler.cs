using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.GetMultipleTablesRows;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTablesQueryHandler(IMasterDataLoaderCreator masterDataLoaderCreator)
    : IQueryHandler<MdGetTablesRequestQuery, MdGetTablesQueryResponse>
{
    public async Task<Result<MdGetTablesQueryResponse>> Handle(MdGetTablesRequestQuery request,
        CancellationToken cancellationToken)
    {
        List<string?> tableNames =
            [.. request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()];
        var mdLoader = new MasterDataLoader(tableNames, masterDataLoaderCreator);
        Result<Dictionary<string, IEnumerable<dynamic>>> loaderResult = await mdLoader.Run(cancellationToken);
        if (loaderResult.IsFailure)
        {
            return Result.Failure<MdGetTablesQueryResponse>(loaderResult.Error);
        }

        return new MdGetTablesQueryResponse(loaderResult.Value);
    }
}

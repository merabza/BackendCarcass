using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.GetLookupTables;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    GetLookupTablesQueryHandler : IQueryHandler<MdGetLookupTablesRequestQuery, MdGetLookupTablesQueryResponse>
{
    private readonly IReturnValuesLoaderCreator _returnValuesLoaderCreator;
    private readonly IReturnValuesRepository _rvRepo;

    public GetLookupTablesQueryHandler(IReturnValuesRepository rvRepo,
        IReturnValuesLoaderCreator returnValuesLoaderCreator)
    {
        _rvRepo = rvRepo;
        _returnValuesLoaderCreator = returnValuesLoaderCreator;
    }

    public async Task<OneOf<MdGetLookupTablesQueryResponse, Err[]>> Handle(MdGetLookupTablesRequestQuery request,
        CancellationToken cancellationToken)
    {
        //var reqQuery = request.HttpRequest.Query["tables"];
        List<string> tableNames = request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()
            .ToList()!;
        var mdLoader = new ReturnValuesLoader(tableNames, _rvRepo, _returnValuesLoaderCreator);
        OneOf<Dictionary<string, IEnumerable<SrvModel>>, Err[]> loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetLookupTablesQueryResponse, Err[]>>(
            r => new MdGetLookupTablesQueryResponse(r), e => e.ToArray());
    }
}

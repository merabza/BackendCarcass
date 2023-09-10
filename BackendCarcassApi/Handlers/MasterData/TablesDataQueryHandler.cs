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
public sealed class TablesDataQueryHandler : IQueryHandler<MdTablesDataQueryRequest, MdTablesDataQueryResponse>
{
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;

    public TablesDataQueryHandler(IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<MdTablesDataQueryResponse, IEnumerable<Err>>> Handle(MdTablesDataQueryRequest request,
        CancellationToken cancellationToken)
    {
        Dictionary<string, IEnumerable<dynamic>> resultList = new();
        var reqQuery = request.HttpRequest.Query["tables"];
        var tableNames = reqQuery.Distinct().ToList();

        var errors = new List<Err>();
        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var tableName in tableNames.Where(tableName => tableName is not null))
        {
            var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(tableName!);
            var tableResult = await loader.GetAllRecords(cancellationToken);
            if (tableResult.IsT1)
            {
                errors.AddRange(tableResult.AsT1);
            }
            else
            {
                var res = tableResult.AsT0.Select(s => s.EditFields());
                resultList.Add(tableName!, res);
            }
        }

        if (errors.Count > 0)
            return errors;
        return new MdTablesDataQueryResponse(resultList);
    }
}
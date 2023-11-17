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
public sealed class GetTablesQueryHandler
    (IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator) : IQueryHandler<MdGetTablesQueryRequest,
        MdGetTablesQueryResponse>
{
    public async Task<OneOf<MdGetTablesQueryResponse, IEnumerable<Err>>> Handle(MdGetTablesQueryRequest request,
        CancellationToken cancellationToken)
    {
        var resultList = new Dictionary<string, IEnumerable<dynamic>>();
        var reqQuery = request.HttpRequest.Query["tables"];
        var tableNames = reqQuery.Distinct().ToList();

        var errors = new List<Err>();
        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var tableName in tableNames.Where(tableName => tableName is not null))
        {
            var loader = masterDataLoaderCrudCreator.CreateMasterDataLoader(tableName!);
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
        return new MdGetTablesQueryResponse(resultList);
    }
}
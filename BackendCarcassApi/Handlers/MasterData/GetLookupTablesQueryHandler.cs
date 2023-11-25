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
    (IReturnValuesRepository rvRepo) : IQueryHandler<MdGetLookupTablesQueryRequest, MdGetLookupTablesQueryResponse>
{
    public async Task<OneOf<MdGetLookupTablesQueryResponse, IEnumerable<Err>>> Handle(
        MdGetLookupTablesQueryRequest request, CancellationToken cancellationToken)
    {
        //var resultList = new Dictionary<string, IEnumerable<ReturnValueModel>>();
        var reqQuery = request.HttpRequest.Query["tables"];
        List<string> tableNames = reqQuery.Where(tableName => tableName is not null).Distinct().ToList()!;
        var mdLoader = new MasterDataReturnValuesLoader(tableNames, rvRepo);
        var loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetLookupTablesQueryResponse, IEnumerable<Err>>>(
            r => new MdGetLookupTablesQueryResponse(r), e => (Err[])e);


        //if (loaderResult..Count > 0)
        //    return errors;
        //return new MdGetLookupTablesQueryResponse(loaderResult);

        //var errors = new List<Err>();
        ////ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        //foreach (var tableName in tableNames.Where(tableName => tableName is not null))
        //{
        //    var loader = masterDataLoaderCrudCreator.CreateMasterDataLoader(tableName!);
        //    var tableResult = await loader.GetAllRecords(cancellationToken);
        //    if (tableResult.IsT1)
        //    {
        //        errors.AddRange(tableResult.AsT1);
        //    }
        //    else
        //    {
        //        var res = tableResult.AsT0.Select(s => s.EditFields());
        //        resultList.Add(tableName!, res);
        //    }
        //}

        //if (errors.Count > 0)
        //    return errors;
        //return new MdGetLookupTablesQueryResponse(resultList);
    }
}
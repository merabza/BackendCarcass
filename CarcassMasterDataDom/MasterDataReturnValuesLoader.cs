using OneOf;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using SystemToolsShared;
using System.Threading;
// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace CarcassMasterDataDom;

public class MasterDataReturnValuesLoader(List<string> tableNames, IReturnValuesRepository rvRepo)
{
    private readonly List<string> _tableNames = tableNames;
    private readonly IReturnValuesRepository _rvRepo = rvRepo;

    public async Task<OneOf<Dictionary<string, IEnumerable<SrvModel>>, IEnumerable<Err>>> Run(
        CancellationToken cancellationToken)
    {
        var resultList = new Dictionary<string, IEnumerable<SrvModel>>();
        var tableDataTypes = await _rvRepo.GetDataTypesByTableNames(_tableNames, cancellationToken);
        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var dt in tableDataTypes)
        {
            var oneTableReturnValues = await _rvRepo.GetAllSimpleReturnValues(dt, cancellationToken);
            resultList.Add(dt.DtTable, oneTableReturnValues);
        }

        return resultList;
    }

}
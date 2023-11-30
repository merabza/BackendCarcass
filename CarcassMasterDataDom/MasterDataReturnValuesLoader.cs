using OneOf;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using SystemToolsShared;
using System.Threading;

namespace CarcassMasterDataDom;

public class MasterDataReturnValuesLoader
{
    private readonly List<string> _tableNames;
    private readonly IReturnValuesRepository _rvRepo;

    public MasterDataReturnValuesLoader(List<string> tableNames, IReturnValuesRepository rvRepo)
    {
        _tableNames = tableNames;
        _rvRepo = rvRepo;
    }

    public async Task<OneOf<Dictionary<string, IEnumerable<ReturnValueModel>>, IEnumerable<Err>>> Run(
        CancellationToken cancellationToken)
    {
        var resultList = new Dictionary<string, IEnumerable<ReturnValueModel>>();
        var tableDataTypes = await _rvRepo.GetDataTypesByTableNames(_tableNames, cancellationToken);
        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var dt in tableDataTypes)
        {
            var oneTableReturnValues = await _rvRepo.GetAllReturnValues(dt, cancellationToken);
            resultList.Add(dt.DtTable, oneTableReturnValues);
        }

        return resultList;
    }

}
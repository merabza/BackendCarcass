using OneOf;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using SystemToolsShared;

namespace CarcassMasterDataDom;

public class MasterDataReturnValuesLoader(List<string> tableNames, IReturnValuesRepository rvRepo)
{
    public async Task<OneOf<Dictionary<string, IEnumerable<ReturnValueModel>>, IEnumerable<Err>>> Run()
    {
        var resultList = new Dictionary<string, IEnumerable<ReturnValueModel>>();
        var tableDataTypes = await rvRepo.GetDataTypesByTableNames(tableNames);
        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var dt in tableDataTypes)
        {
            var oneTableReturnValues = await rvRepo.GetAllReturnValues(dt);
            resultList.Add(dt.DtTable, oneTableReturnValues);
        }

        return resultList;
    }



}
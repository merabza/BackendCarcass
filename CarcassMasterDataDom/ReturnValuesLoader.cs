using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom.Models;
using OneOf;
using SystemToolsShared;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace CarcassMasterDataDom;

public class ReturnValuesLoader(
    List<string> tableNames,
    IReturnValuesRepository rvRepo,
    IReturnValuesLoaderCreator returnValuesLoaderCreator)
{
    private readonly IReturnValuesLoaderCreator _returnValuesLoaderCreator = returnValuesLoaderCreator;
    private readonly IReturnValuesRepository _rvRepo = rvRepo;
    private readonly List<string> _tableNames = tableNames;

    public async Task<OneOf<Dictionary<string, IEnumerable<SrvModel>>, IEnumerable<Err>>> Run(
        CancellationToken cancellationToken)
    {
        var resultList = new Dictionary<string, IEnumerable<SrvModel>>();
        var tableDataTypes = await _rvRepo.GetDataTypesByTableNames(_tableNames, cancellationToken);
        var errors = new List<Err>();

        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var dt in tableDataTypes)
        {
            var loader =
                new MasterDataReturnValuesLoader(dt,
                    _rvRepo); // _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(dt);
            var tableResult = await loader.GetSimpleReturnValues(cancellationToken);
            if (tableResult.IsT1)
            {
                errors.AddRange(tableResult.AsT1);
            }
            else
            {
                var res = tableResult.AsT0;
                resultList.Add(dt.DtTable, res);
            }
        }

        var tablesWithoutDataType = _tableNames.Except(tableDataTypes.Select(x => x.DtTable));
        foreach (var tableName in tablesWithoutDataType)
        {
            var loader = _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(tableName);
            if (loader is null)
            {
                errors.Add(MasterDataApiErrors
                    .LoaderForTableNotFound(tableName)); //ჩამტვირთავი ცხრილისთვის სახელით {tableName} ვერ მოიძებნა
                continue;
            }

            var tableResult = await loader.GetSimpleReturnValues(cancellationToken);
            if (tableResult.IsT1)
            {
                errors.AddRange(tableResult.AsT1);
            }
            else
            {
                var res = tableResult.AsT0;
                resultList.Add(tableName, res);
            }
        }


        if (errors.Count > 0)
            return errors;
        return resultList;
    }
}
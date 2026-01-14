using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassMasterData.Models;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterData;

public sealed class ReturnValuesLoader
{
    private readonly IReturnValuesLoaderCreator _returnValuesLoaderCreator;
    private readonly IReturnValuesRepository _rvRepo;
    private readonly List<string> _tableNames;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ReturnValuesLoader(List<string> tableNames, IReturnValuesRepository rvRepo,
        IReturnValuesLoaderCreator returnValuesLoaderCreator)
    {
        _returnValuesLoaderCreator = returnValuesLoaderCreator;
        _rvRepo = rvRepo;
        _tableNames = tableNames;
    }

    public async Task<OneOf<Dictionary<string, IEnumerable<SrvModel>>, Err[]>> Run(
        CancellationToken cancellationToken = default)
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
        foreach (string tableName in tablesWithoutDataType)
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
        {
            return errors.ToArray();
        }

        return resultList;
    }
}

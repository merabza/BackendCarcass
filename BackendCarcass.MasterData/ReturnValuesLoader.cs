using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.BackendCarcassContracts.Errors;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

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
        List<DataTypeModelForRvs> tableDataTypes =
            await _rvRepo.GetDataTypesByTableNames(_tableNames, cancellationToken);
        var errors = new List<Err>();

        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (DataTypeModelForRvs dt in tableDataTypes)
        {
            var loader =
                new MasterDataReturnValuesLoader(dt,
                    _rvRepo); // _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(dt);
            OneOf<IEnumerable<SrvModel>, Err[]> tableResult = await loader.GetSimpleReturnValues(cancellationToken);
            if (tableResult.IsT1)
            {
                errors.AddRange(tableResult.AsT1);
            }
            else
            {
                IEnumerable<SrvModel>? res = tableResult.AsT0;
                resultList.Add(dt.DtTable, res);
            }
        }

        IEnumerable<string> tablesWithoutDataType = _tableNames.Except(tableDataTypes.Select(x => x.DtTable));
        foreach (string tableName in tablesWithoutDataType)
        {
            IReturnValuesLoader? loader = _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(tableName);
            if (loader is null)
            {
                errors.Add(MasterDataApiErrors
                    .LoaderForTableNotFound(tableName)); //ჩამტვირთავი ცხრილისთვის სახელით {tableName} ვერ მოიძებნა
                continue;
            }

            OneOf<IEnumerable<SrvModel>, Err[]> tableResult = await loader.GetSimpleReturnValues(cancellationToken);
            if (tableResult.IsT1)
            {
                errors.AddRange(tableResult.AsT1);
            }
            else
            {
                IEnumerable<SrvModel>? res = tableResult.AsT0;
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

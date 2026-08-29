using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcassShared.Contracts.Errors;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.MasterData;

public sealed class ReturnValuesLoader
{
    private readonly IReturnValuesLoaderCreator _returnValuesLoaderCreator;
    private readonly IReturnValuesRepository _rvRepo;
    private readonly List<string?> _tableNames;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ReturnValuesLoader(List<string?> tableNames, IReturnValuesRepository rvRepo,
        IReturnValuesLoaderCreator returnValuesLoaderCreator)
    {
        _returnValuesLoaderCreator = returnValuesLoaderCreator;
        _rvRepo = rvRepo;
        _tableNames = tableNames;
    }

    public async Task<Result<Dictionary<string, IEnumerable<SrvModel>>>> Run(
        CancellationToken cancellationToken = default)
    {
        var resultList = new Dictionary<string, IEnumerable<SrvModel>>();
        List<DataTypeModelForRvs> tableDataTypes =
            await _rvRepo.GetDataTypesByTableNames(_tableNames, cancellationToken);
        var errors = new List<Error>();

        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (DataTypeModelForRvs dt in tableDataTypes)
        {
            var loader =
                new MasterDataReturnValuesLoader(dt,
                    _rvRepo); // _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(dt);
            Result<IEnumerable<SrvModel>> tableResult = await loader.GetSimpleReturnValues(cancellationToken);
            if (tableResult.IsFailure)
            {
                errors.Add(tableResult.Error);
            }
            else
            {
                IEnumerable<SrvModel> res = tableResult.Value;
                resultList.Add(dt.DtTable, res);
            }
        }

        IEnumerable<string?> tablesWithoutDataType = _tableNames.Except(tableDataTypes.Select(x => x.DtTable));
        foreach (string? tableName in tablesWithoutDataType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                continue;
            }

            IReturnValuesLoader? loader = _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(tableName);
            if (loader is null)
            {
                errors.Add(MasterDataApiErrors.LoaderForTableNotFound(tableName)
                    .ToError()); //ჩამტვირთავი ცხრილისთვის სახელით {tableName} ვერ მოიძებნა
                continue;
            }

            Result<IEnumerable<SrvModel>> tableResult = await loader.GetSimpleReturnValues(cancellationToken);
            if (tableResult.IsFailure)
            {
                errors.Add(tableResult.Error);
            }
            else
            {
                IEnumerable<SrvModel> res = tableResult.Value;
                resultList.Add(tableName, res);
            }
        }

        if (errors.Count > 0)
        {
            return Result.Failure<Dictionary<string, IEnumerable<SrvModel>>>(errors.Count == 1
                ? errors[0]
                : new ValidationError([.. errors]));
        }

        return resultList;
    }
}

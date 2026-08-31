using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Domain;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData;

public sealed class MasterDataLoader
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCreator;
    private readonly List<string?> _tableNames;

    public MasterDataLoader(List<string?> tableNames, IMasterDataLoaderCreator masterDataLoaderCreator)
    {
        _masterDataLoaderCreator = masterDataLoaderCreator;
        _tableNames = tableNames;
    }

    public async ValueTask<Result<Dictionary<string, IEnumerable<dynamic>>>> Run(
        CancellationToken cancellationToken = default)
    {
        var resultList = new Dictionary<string, IEnumerable<dynamic>>();
        var errors = new List<Error>();

        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (string tableName in _tableNames)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                continue;
            }

            Result<IMasterDataLoader> createMasterDataLoaderResult =
                _masterDataLoaderCreator.CreateMasterDataLoader(tableName);
            if (createMasterDataLoaderResult.IsFailure)
            {
                return Result.Failure<Dictionary<string, IEnumerable<dynamic>>>(createMasterDataLoaderResult.Error);
            }

            IMasterDataLoader loader = createMasterDataLoaderResult.Value;
            Result<IEnumerable<IDataType>> tableResult = await loader.GetAllRecords(cancellationToken);
            if (tableResult.IsFailure)
            {
                errors.Add(tableResult.Error);
            }
            else
            {
                IEnumerable<dynamic> res = tableResult.Value.Select(s => s.EditFields());
                resultList.Add(tableName, res);
            }
        }

        if (errors.Count > 0)
        {
            return Result.Failure<Dictionary<string, IEnumerable<dynamic>>>(errors.Count == 1
                ? errors[0]
                : new ValidationError([.. errors]));
        }

        return resultList;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public sealed class MasterDataLoader
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCreator;
    private readonly List<string> _tableNames;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataLoader(List<string> tableNames, IMasterDataLoaderCreator masterDataLoaderCreator)
    {
        _masterDataLoaderCreator = masterDataLoaderCreator;
        _tableNames = tableNames;
    }

    public async ValueTask<OneOf<Dictionary<string, IEnumerable<dynamic>>, Err[]>> Run(
        CancellationToken cancellationToken = default)
    {
        var resultList = new Dictionary<string, IEnumerable<dynamic>>();
        var errors = new List<Err>();

        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var tableName in _tableNames)
        {
            var createMasterDataLoaderResult = _masterDataLoaderCreator.CreateMasterDataLoader(tableName);
            if (createMasterDataLoaderResult.IsT1) return createMasterDataLoaderResult.AsT1;

            var loader = createMasterDataLoaderResult.AsT0;
            var tableResult = await loader.GetAllRecords(cancellationToken);
            if (tableResult.IsT1)
            {
                errors.AddRange(tableResult.AsT1);
            }
            else
            {
                var res = tableResult.AsT0.Select(s => s.EditFields());
                resultList.Add(tableName, res);
            }
        }

        if (errors.Count > 0) return errors.ToArray();

        return resultList;
    }
}
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemToolsShared;
using System.Threading;

// ReSharper disable ConvertToPrimaryConstructor

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace CarcassMasterDataDom;

public class MasterDataLoader
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCreator;
    private readonly List<string> _tableNames;
    private readonly IReturnValuesRepository _rvRepo;

    public MasterDataLoader(List<string> tableNames, IReturnValuesRepository rvRepo,
        IMasterDataLoaderCreator masterDataLoaderCreator)
    {
        _masterDataLoaderCreator = masterDataLoaderCreator;
        _tableNames = tableNames;
        _rvRepo = rvRepo;
    }


    public async Task<OneOf<Dictionary<string, IEnumerable<dynamic>>, IEnumerable<Err>>> Run(
        CancellationToken cancellationToken)
    {
        var resultList = new Dictionary<string, IEnumerable<dynamic>>();
        var errors = new List<Err>();

        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var tableName in _tableNames)
        {
            var loader = _masterDataLoaderCreator.CreateMasterDataLoader(tableName!);
            var tableResult = await loader.GetAllRecords(cancellationToken);
            if (tableResult.IsT1)
            {
                errors.AddRange(tableResult.AsT1);
            }
            else
            {
                var res = tableResult.AsT0.Select(s => s.EditFields());
                resultList.Add(tableName!, res);
            }
        }


        if (errors.Count > 0)
            return errors;
        return resultList;
    }
}
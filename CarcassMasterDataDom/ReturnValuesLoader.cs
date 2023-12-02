using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using SystemToolsShared;
using System.Threading;
using CarcassContracts.ErrorModels;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace CarcassMasterDataDom;

public class ReturnValuesLoader(List<string> tableNames, IReturnValuesRepository rvRepo, IReturnValuesLoaderCreator returnValuesLoaderCreator)
{
    private readonly IReturnValuesLoaderCreator _returnValuesLoaderCreator = returnValuesLoaderCreator;
    private readonly List<string> _tableNames = tableNames;
    private readonly IReturnValuesRepository _rvRepo = rvRepo;

    public async Task<OneOf<Dictionary<string, IEnumerable<SrvModel>>, IEnumerable<Err>>> Run(
        CancellationToken cancellationToken)
    {
        var resultList = new Dictionary<string, IEnumerable<SrvModel>>();
        var tableDataTypes = await _rvRepo.GetDataTypesByTableNames(_tableNames, cancellationToken);
        var errors = new List<Err>();

        //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
        foreach (var dt in tableDataTypes)
        {
            

            var loader = new MasterDataReturnValuesLoader(dt, _rvRepo);// _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(dt);
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



            //if (!Enum.TryParse(tableName.CapitalizeCamel(), out EAgrMdQueryNames qName))
            //    return base.CreateMasterDataLoader(queryName);

            //List<SrvModel> oneTableReturnValues;

            //switch (dt.DtTable)
            //{


            //}

            //oneTableReturnValues = await _rvRepo.GetSimpleReturnValues(dt, cancellationToken);
            //resultList.Add(dt.DtTable, oneTableReturnValues);
        }

        var tablesWithoutDataType = _tableNames.Except(tableDataTypes.Select(x => x.DtTable));
        foreach (var tableName in tablesWithoutDataType)
        {
            var loader = _returnValuesLoaderCreator.CreateReturnValuesLoaderLoader(tableName);
            if ( loader is null)
            {
                errors.Add(MasterDataApiErrors.LoaderForTableNotFound(tableName)); //ჩამტვირთავი ცხრილისთვის სახელით {tableName} ვერ მოიძებნა
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


        //return resultList;
    }

}
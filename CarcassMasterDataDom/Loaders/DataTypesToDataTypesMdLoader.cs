using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom.Loaders;

public sealed class DataTypesToDataTypesMdLoader : IMasterDataLoader
{
    private readonly IDataTypesRepository _dataTypesRepository;


    public DataTypesToDataTypesMdLoader(IDataTypesRepository dataTypesRepository)
    {
        _dataTypesRepository = dataTypesRepository;
    }

    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken)
    {
        var result = await _dataTypesRepository.LoadDataTypesToDataTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }

    public Task<OneOf<TableRowsData, Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords()
    {
        var result = await _dataTypesRepository.LoadDataTypesToDataTypes();
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom.Loaders;

public sealed class DataTypesToDataTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken)
    {
        var result = await dataTypesRepository.LoadDataTypesToDataTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Loaders;

public sealed class DataTypesToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        var result = await dataTypesRepository.LoadDataTypesToCrudTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}

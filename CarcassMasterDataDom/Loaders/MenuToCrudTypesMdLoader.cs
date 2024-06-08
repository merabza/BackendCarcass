using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom.Loaders;

public sealed class MenuToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken)
    {
        var result = await dataTypesRepository.LoadMenuToCrudTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}
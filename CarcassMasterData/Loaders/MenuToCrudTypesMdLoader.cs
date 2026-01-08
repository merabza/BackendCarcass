using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterData.Loaders;

public sealed class MenuToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        var result = await dataTypesRepository.LoadMenuToCrudTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}
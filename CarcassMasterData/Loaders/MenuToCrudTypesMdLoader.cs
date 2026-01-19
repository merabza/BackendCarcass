using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterData.Models;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace CarcassMasterData.Loaders;

public sealed class MenuToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        IEnumerable<MenuToCrudTypeDomModel> result = await dataTypesRepository.LoadMenuToCrudTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}

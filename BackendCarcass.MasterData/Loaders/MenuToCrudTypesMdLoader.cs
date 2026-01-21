using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Loaders;

public sealed class MenuToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        IEnumerable<MenuToCrudTypeDomModel> result = await dataTypesRepository.LoadMenuToCrudTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}

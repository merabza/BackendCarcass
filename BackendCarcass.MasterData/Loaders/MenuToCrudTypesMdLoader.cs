using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using BackendCarcassDomain.Entities;
using SystemTools.SharedKernel;

namespace BackendCarcass.MasterData.Loaders;

public sealed class MenuToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(CancellationToken cancellationToken = default)
    {
        IEnumerable<MenuToCrudTypeDomModel> result = await dataTypesRepository.LoadMenuToCrudTypes(cancellationToken);
        return Result.Success<IEnumerable<IDataType>>(result);
    }
}

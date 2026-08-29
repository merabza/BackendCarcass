using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using BackendCarcassDomain.Entities;
using SystemTools.SharedKernel;

namespace BackendCarcass.MasterData.Loaders;

public sealed class DataTypesToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(CancellationToken cancellationToken = default)
    {
        IEnumerable<DataTypeToCrudTypeDomModel> result =
            await dataTypesRepository.LoadDataTypesToCrudTypes(cancellationToken);
        return Result.Success<IEnumerable<IDataType>>(result);
    }
}

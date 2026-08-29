using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Domain;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.Loaders;

public sealed class DataTypesToDataTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(CancellationToken cancellationToken = default)
    {
        IEnumerable<DataTypeToDataTypeDomModel> result =
            await dataTypesRepository.LoadDataTypesToDataTypes(cancellationToken);
        return Result.Success<IEnumerable<IDataType>>(result);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using BackendCarcassDomain.Entities;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Loaders;

public sealed class DataTypesToDataTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<OneOf<IEnumerable<IDataType>, ErrorOmd[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        IEnumerable<DataTypeToDataTypeDomModel> result =
            await dataTypesRepository.LoadDataTypesToDataTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, ErrorOmd[]>.FromT0(result);
    }
}

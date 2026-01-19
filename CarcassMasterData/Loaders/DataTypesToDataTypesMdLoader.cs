using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterData.Models;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace CarcassMasterData.Loaders;

public sealed class DataTypesToDataTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        IEnumerable<DataTypeToDataTypeDomModel> result =
            await dataTypesRepository.LoadDataTypesToDataTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}

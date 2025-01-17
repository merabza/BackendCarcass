using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom.Loaders;

public sealed class DataTypesToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository) : IMasterDataLoader
{
    public async ValueTask<OneOf<IEnumerable<IDataType>, IEnumerable<Err>>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        var result = await dataTypesRepository.LoadDataTypesToCrudTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, IEnumerable<Err>>.FromT0(result);
    }
}
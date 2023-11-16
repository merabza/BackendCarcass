using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom.Loaders;

public sealed class MenuToCrudTypesMdLoader : IMasterDataLoader
{
    private readonly IDataTypesRepository _dataTypesRepository;

    public MenuToCrudTypesMdLoader(IDataTypesRepository dataTypesRepository)
    {
        _dataTypesRepository = dataTypesRepository;
    }

    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken)
    {
        var result = await _dataTypesRepository.LoadMenuToCrudTypes(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(result);
    }
}
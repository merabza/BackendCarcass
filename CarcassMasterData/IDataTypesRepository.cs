using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterData.Models;

namespace CarcassMasterData;

public interface IDataTypesRepository
{
    Task<IEnumerable<MenuToCrudTypeDomModel>> LoadMenuToCrudTypes(CancellationToken cancellationToken = default);

    Task<IEnumerable<DataTypeToCrudTypeDomModel>> LoadDataTypesToCrudTypes(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DataTypeToDataTypeDomModel>> LoadDataTypesToDataTypes(
        CancellationToken cancellationToken = default);
}
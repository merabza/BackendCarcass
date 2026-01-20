using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;

namespace BackendCarcass.MasterData;

public interface IDataTypesRepository
{
    Task<IEnumerable<MenuToCrudTypeDomModel>> LoadMenuToCrudTypes(CancellationToken cancellationToken = default);

    Task<IEnumerable<DataTypeToCrudTypeDomModel>> LoadDataTypesToCrudTypes(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DataTypeToDataTypeDomModel>> LoadDataTypesToDataTypes(
        CancellationToken cancellationToken = default);
}
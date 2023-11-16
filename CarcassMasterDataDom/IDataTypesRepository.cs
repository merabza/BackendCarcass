using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;

namespace CarcassMasterDataDom;

public interface IDataTypesRepository
{
    Task<IEnumerable<MenuToCrudTypeDomModel>> LoadMenuToCrudTypes(CancellationToken cancellationToken);
    Task<IEnumerable<DataTypeToCrudTypeDomModel>> LoadDataTypesToCrudTypes(CancellationToken cancellationToken);
    Task<IEnumerable<DataTypeToDataTypeDomModel>> LoadDataTypesToDataTypes(CancellationToken cancellationToken);
}
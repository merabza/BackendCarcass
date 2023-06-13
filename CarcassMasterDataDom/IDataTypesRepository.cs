using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;

namespace CarcassMasterDataDom;

public interface IDataTypesRepository
{
    Task<IEnumerable<DataTypeToCrudTypeDomModel>> LoadDataTypesToCrudTypes();
    Task<IEnumerable<DataTypeToDataTypeDomModel>> LoadDataTypesToDataTypes();
}
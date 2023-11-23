using CarcassMasterDataDom.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarcassMasterDataDom;

public interface IReturnValuesRepository
{
    Task<List<DataTypeModel>> GetDataTypesByTableNames(List<string> tableNames);
    Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModel dt);
}
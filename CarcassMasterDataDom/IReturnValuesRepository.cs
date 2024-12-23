using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;

namespace CarcassMasterDataDom;

public interface IReturnValuesRepository
{
    Task<List<DataTypeModelForRvs>> GetDataTypesByTableNames(List<string> tableNames,
        CancellationToken cancellationToken = default);

    Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken = default);
    ValueTask<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken = default);
}
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using CarcassMasterDataDom.Models;
using SystemToolsShared;
using OneOf;

namespace CarcassMasterDataDom;

public interface IReturnValuesLoader
{
    Task<OneOf<IEnumerable<SrvModel>, Err[]>> GetSimpleReturnValues(CancellationToken cancellationToken);
    //Task<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken);
}
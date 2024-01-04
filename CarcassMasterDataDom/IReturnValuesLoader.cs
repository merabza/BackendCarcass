using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom;

public interface IReturnValuesLoader
{
    Task<OneOf<IEnumerable<SrvModel>, Err[]>> GetSimpleReturnValues(CancellationToken cancellationToken);
    //Task<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken);
}
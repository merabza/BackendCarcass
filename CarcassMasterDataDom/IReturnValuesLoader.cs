using CarcassMasterDataDom.Models;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom;

public interface IReturnValuesLoader
{
    Task<OneOf<IEnumerable<SrvModel>, Err[]>> GetSimpleReturnValues(CancellationToken cancellationToken);
    //Task<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken);
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterData.Models;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterData;

public interface IReturnValuesLoader
{
    Task<OneOf<IEnumerable<SrvModel>, Err[]>> GetSimpleReturnValues(CancellationToken cancellationToken = default);
    //Task<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken = default);
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public interface IReturnValuesLoader
{
    Task<OneOf<IEnumerable<SrvModel>, Err[]>> GetSimpleReturnValues(CancellationToken cancellationToken = default);
    //Task<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken = default);
}

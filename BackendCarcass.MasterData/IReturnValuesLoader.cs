using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using SystemTools.SharedKernel;

namespace BackendCarcass.MasterData;

public interface IReturnValuesLoader
{
    Task<Result<IEnumerable<SrvModel>>> GetSimpleReturnValues(CancellationToken cancellationToken = default);
    //Task<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken = default);
}

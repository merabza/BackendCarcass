using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassDomain.Entities;
using SystemTools.SharedKernel;

namespace BackendCarcass.MasterData;

public interface IMasterDataLoader
{
    ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(CancellationToken cancellationToken = default);
}

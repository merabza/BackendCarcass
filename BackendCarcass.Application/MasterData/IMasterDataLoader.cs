using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Domain;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData;

public interface IMasterDataLoader
{
    ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(CancellationToken cancellationToken = default);
}

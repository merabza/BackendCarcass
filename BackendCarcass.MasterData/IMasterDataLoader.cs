using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public interface IMasterDataLoader
{
    ValueTask<OneOf<IEnumerable<IDataType>, Error[]>> GetAllRecords(CancellationToken cancellationToken = default);
}

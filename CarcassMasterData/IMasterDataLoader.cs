using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterData;

public interface IMasterDataLoader
{
    ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken = default);
}
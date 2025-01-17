using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom;

public interface IMasterDataLoader
{
    ValueTask<OneOf<IEnumerable<IDataType>, IEnumerable<Err>>> GetAllRecords(CancellationToken cancellationToken = default);
}
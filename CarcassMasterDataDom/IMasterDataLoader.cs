using System.Collections.Generic;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom;

public interface IMasterDataLoader
{
    Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords();
}
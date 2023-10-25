using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom;

public interface IMasterDataLoader
{
    Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken);
    Task<OneOf<TableRowsData,Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest, CancellationToken cancellationToken);
}
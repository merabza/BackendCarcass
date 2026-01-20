using System.Threading;
using System.Threading.Tasks;

namespace BackendCarcass.MasterData.SortIdStuff;

public interface ISortIdHelper
{
    Task ReSortSortIds(object query, CancellationToken cancellationToken = default);
    int CountSortIdMax(object query);
    int CountItems(object query);
    Task<bool> IsSortIdExists(object query, int sortId, int exceptId);

    Task IncreaseSortIds(object query, int fromSortId, int increaseWith, int exceptId,
        CancellationToken cancellationToken = default);
}

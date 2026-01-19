using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CarcassMasterData.SortIdStuff;

public sealed class SortIdHelper<T> : ISortIdHelper where T : class, ISortedDataType
{
    private readonly ICarcassMasterDataRepository _cmdRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SortIdHelper(ICarcassMasterDataRepository cmdRepo)
    {
        _cmdRepo = cmdRepo;
    }

    //3. დავადგინოთ არის თუ არა ისეთი ჩანაწერები, რომლებიც იწვევს SortId-ის ჩავარდნას და გამოვასწოროთ ჩავარდნები.
    //3.1 უნდა ჩავტვირთოთ იდენტიფიკატორები, SortId-ები, RowId-ები დალაგებული SortId-ებით
    //3.2. ისეთი ჩანაწერებისათვის რომლებისთვისაც SortId != RowId, გავაახლოთ SortId, RowId-ის მნიშვნელობით.
    public async Task ReSortSortIds(object query, CancellationToken cancellationToken = default)
    {
        List<T> sortedList = await ((IQueryable<T>)query).OrderBy(x => x.SortId).ToListAsync(cancellationToken);
        foreach (var item in sortedList.Select((s, i) => new { s, i }).Where(x => x.s.SortId != x.i))
        {
            item.s.SortId = item.i;
            _cmdRepo.Update(item.s);
        }
    }

    //public async Task ReSortSortIds(object query, CancellationToken cancellationToken = default)
    //{
    //    var tQuery = (IQueryable<T>)query;
    //    var forUpdateList = await tQuery.OrderBy(x => x.SortId).ToListAsync(cancellationToken);

    //    var i = 0;
    //    foreach (var item in forUpdateList)
    //    {
    //        item.SortId = i;
    //        i++;
    //        _cmdRepo.Update(item);
    //    }
    //}

    //არსებული SortId-ების მაქსიმუმის დათვლა
    public int CountSortIdMax(object query)
    {
        var tQuery = (IQueryable<T>)query;
        return tQuery.Max(x => x.SortId);
    }

    public int CountItems(object query)
    {
        var tQuery = (IQueryable<T>)query;
        return tQuery.Count();
    }

    //ვიპოვოთ SortId-ის შესაბამისი ჩანაწერი არსებობს თუ არა ცხრილში. (ოღონდ ეს ჩანაწერი უნდა იყოს დასარედაქტირებელი ჩანაწერისგან განსხვავებული)
    public async Task<bool> IsSortIdExists(object query, int sortId, int exceptId)
    {
        var tQuery = (IQueryable<T>)query;
        List<T> ft = await tQuery.Where(x => x.SortId == sortId).ToListAsync();
        return ft.Any(x => x.Id != exceptId);
    }

    //ყველა ჩანაწერი, რომლი SortId >= შესანახ SortId-ს, ყველას გავუზარდოთ 1-ით
    public async Task IncreaseSortIds(object query, int fromSortId, int increaseWith, int exceptId,
        CancellationToken cancellationToken = default)
    {
        var tQuery = (IQueryable<T>)query;
        List<T> forUpdateList = await tQuery.Where(x => x.SortId >= fromSortId).OrderBy(x => x.SortId)
            .ToListAsync(cancellationToken);
        foreach (T item in forUpdateList.Where(x => x.Id != exceptId))
        {
            item.SortId += increaseWith;
            _cmdRepo.Update(item);
        }
    }
}

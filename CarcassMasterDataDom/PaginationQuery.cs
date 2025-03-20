using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LibCrud.Models;
using Microsoft.EntityFrameworkCore;
using SystemToolsShared;

namespace CarcassMasterDataDom;

public static class PaginationQuery
{
    public static async Task<(int, int, List<TResult>)> UseCustomSortFilterPagination<TSource, TResult>(
        this IQueryable<TSource> query, FilterSortRequest filterSortRequest,
        Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
        where TSource : class
    {
        var (realOffset, count, preparedQuery) = query.PrepareSortFilterPagination(filterSortRequest);
        var rowsSel = await preparedQuery.Select(selector).ToListAsync(cancellationToken);
        return (realOffset, count, rowsSel);
    }

    private static (int, int, IQueryable<T>) PrepareSortFilterPagination<T>(this IQueryable<T> query,
        FilterSortRequest filterSortRequest) where T : class
    {
        var filters = CustomExpressionFilter.CustomFilter<T>(filterSortRequest.FilterFields);

        if (filters is not null)
            query = query.CustomFilter(filters);

        var count = query.Count();

        var realOffset = filterSortRequest.Offset;
        if (realOffset >= count)
            realOffset = (count - filterSortRequest.RowsCount) / filterSortRequest.RowsCount *
                         filterSortRequest.RowsCount;

        if (realOffset < 0) realOffset = 0;

        var realRowsCountToLoad = realOffset + filterSortRequest.RowsCount > count
            ? count - realOffset
            : filterSortRequest.RowsCount;

        query = query.CustomSort(filterSortRequest.SortByFields);
        query = query.CustomPagination(realOffset, realRowsCountToLoad);
        return (realOffset, count, query);
    }

    private static IQueryable<T> CustomFilter<T>(this IQueryable<T> query, Expression<Func<T, bool>>? filter = null)
        where T : class
    {
        if (filter is not null) query = query.Where(filter);

        return query;
    }

    private static IQueryable<T> CustomSort<T>(this IQueryable<T> query, SortField[]? sortByFields) where T : class
    {
        if (sortByFields is not null && sortByFields.Length != 0)
            query = sortByFields.Aggregate(query,
                (current, field) =>
                    current.OrderBy(field.FieldName.CapitalizeCamel(), field.Ascending, field.PropObjType));
        return query;
    }

    private static IQueryable<T> CustomPagination<T>(this IQueryable<T> query, int offset = 0, int pageSize = 10)
    {
        query = query.Skip(offset).Take(pageSize);
        return query;
    }
}
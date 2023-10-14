using CarcassDom.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using SystemToolsShared;

namespace CarcassDom;

public static class PaginationQuery
{
    public static IQueryable<T> CustomFilter<T>(this IQueryable<T> query, Expression<Func<T, bool>>? filter = null) where T : class
    {
        if (filter != null)
        {
            query = query.Where(filter);
        }          

        return query;
    }

    public static IQueryable<T> CustomSort<T>(this IQueryable<T> query, SortField[]? sortByFields) where T : class
    {
        if (sortByFields is not null && sortByFields.Any())
            query = sortByFields.Aggregate(query,
                (current, sortByField) =>
                    current.OrderBy(sortByField.FieldName.CapitalizeCamel(), sortByField.Ascending));

        return query;
    }

    public static IQueryable<T> CustomPagination<T>(this IQueryable<T> query, int offset = 0, int pageSize = 10)
    {
        query = query.Skip(offset).Take(pageSize);
        return query;
    }
}
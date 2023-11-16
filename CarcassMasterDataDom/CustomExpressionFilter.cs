using System;
using System.Linq;
using System.Linq.Expressions;
using CarcassMasterDataDom.Models;
using LibCrud.Models;

namespace CarcassMasterDataDom;

public static class CustomExpressionFilter<T> where T : class
{
    public static Expression<Func<T, bool>>? CustomFilter(ColumnFilter[]? columnFilters, string className)
    {

        if (columnFilters is null || !columnFilters.Any())
            return null;

        Expression<Func<T, bool>>? filters;
        try
        {
            var expressionFilters = columnFilters
                .Select(item => new ExpressionFilter { ColumnName = item.FieldName, Value = item.Value }).ToList();
            // Create the parameter expression for the input data
            var parameter = Expression.Parameter(typeof(T), className);

            // Build the filter expression dynamically
            Expression? filterExpression = null;
            foreach (var filter in expressionFilters)
            {
                var property = Expression.Property(parameter, filter.ColumnName);

                Expression comparison;

                if (property.Type == typeof(string))
                {
                    var constant = Expression.Constant(filter.Value);
                    comparison = Expression.Call(property, "Contains", Type.EmptyTypes, constant);
                }
                else if (property.Type == typeof(double))
                {
                    var constant = Expression.Constant(Convert.ToDouble(filter.Value));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(Guid))
                {
                    var constant = Expression.Constant(Guid.Parse(filter.Value));
                    comparison = Expression.Equal(property, constant);
                }
                else
                {
                    var constant = Expression.Constant(Convert.ToInt32(filter.Value));
                    comparison = Expression.Equal(property, constant);
                }


                filterExpression = filterExpression == null
                    ? comparison
                    : Expression.And(filterExpression, comparison);
            }

            if (filterExpression is null)
                return null;
            // Create the lambda expression with the parameter and the filter expression
            filters = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
        }
        catch (Exception)
        {
            filters = null;
        }

        return filters;
    }
}
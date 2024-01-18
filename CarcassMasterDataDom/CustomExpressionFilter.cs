using System;
using System.Linq;
using System.Linq.Expressions;
using CarcassMasterDataDom.Models;
using LibCrud.Models;

namespace CarcassMasterDataDom;

public static class CustomExpressionFilter 
{
    public static Expression<Func<T, bool>>? CustomFilter<T>(ColumnFilter[]? columnFilters) where T : class
    {
        if (columnFilters is null || columnFilters.Length == 0)
            return null;

        Expression<Func<T, bool>>? filters;
        try
        {
            var expressionFilters = columnFilters
                .Select(item => new ExpressionFilter { ColumnName = item.FieldName, Value = item.Value }).ToList();
            // Create the parameter expression for the input data
            var parameter = Expression.Parameter(typeof(T));

            // Build the filter expression dynamically
            Expression? filterExpression = null;
            foreach (var filter in expressionFilters)
            {
                if (filter.ColumnName is null)
                    continue;

                var property = Expression.Property(parameter, filter.ColumnName);

                Expression comparison;

                if (property.Type == typeof(string))
                {
                    var constant = Expression.Constant(filter.Value);
                    comparison = filter.Value is null
                        ? Expression.Call(typeof(string), "IsNullOrEmpty", null, property)
                        : Expression.Call(property, "Contains", Type.EmptyTypes, constant);
                }
                else if (property.Type == typeof(double))
                {
                    var constant = Expression.Constant(Convert.ToDouble(filter.Value));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(Guid))
                {
                    if (filter.Value is null)
                        continue;
                    var constant = Expression.Constant(Guid.Parse(filter.Value));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(int))
                {
                    var constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableInt()), typeof(int));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(int?))
                {
                    var constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableInt()), typeof(int?));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(short))
                {
                    var constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableShort()), typeof(short));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(short?))
                {
                    var constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableShort()), typeof(short?));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(bool))
                {
                    var constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableBool()), typeof(bool));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(bool?))
                {
                    var constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableBool()), typeof(bool?));
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
        catch (Exception e)
        {
            filters = null;
        }

        return filters;
    }


    private static int? ToNullableInt(this string s)
    {
        
        if (int.TryParse(s, out var i)) 
            return i;
        return null;
    }

    private static short? ToNullableShort(this string s)
    {
        
        if (short.TryParse(s, out var i)) 
            return i;
        return null;
    }

    private static bool? ToNullableBool(this string s)
    {
        
        if (bool.TryParse(s, out var i)) 
            return i;
        return null;
    }
}
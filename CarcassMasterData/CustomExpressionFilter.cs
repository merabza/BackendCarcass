using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CarcassMasterData.Models;
using LibCrud.Models;

namespace CarcassMasterData;

public static class CustomExpressionFilter
{
    public static Expression<Func<T, bool>>? CustomFilter<T>(ColumnFilter[]? columnFilters) where T : class
    {
        if (columnFilters is null || columnFilters.Length == 0)
        {
            return null;
        }

        Expression<Func<T, bool>>? filters;
        try
        {
            List<ExpressionFilter> expressionFilters = columnFilters
                .Select(item => new ExpressionFilter { ColumnName = item.FieldName, Value = item.Value }).ToList();
            // Create the parameter expression for the input data
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            // Build the filter expression dynamically
            Expression? filterExpression = null;
            foreach (ExpressionFilter filter in expressionFilters)
            {
                if (filter.ColumnName is null)
                {
                    continue;
                }

                MemberExpression property = Expression.Property(parameter, filter.ColumnName);

                Expression comparison;

                if (property.Type == typeof(string))
                {
                    ConstantExpression constant = Expression.Constant(filter.Value);
                    comparison = filter.Value is null
                        ? Expression.Call(typeof(string), "IsNullOrEmpty", null, property)
                        : Expression.Call(property, "Contains", Type.EmptyTypes, constant);
                }
                else if (property.Type == typeof(double))
                {
                    ConstantExpression constant =
                        Expression.Constant(Convert.ToDouble(filter.Value, CultureInfo.InvariantCulture));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(Guid))
                {
                    if (filter.Value is null)
                    {
                        continue;
                    }

                    ConstantExpression constant = Expression.Constant(Guid.Parse(filter.Value));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(int))
                {
                    UnaryExpression constant =
                        Expression.Convert(Expression.Constant(filter.Value?.ToNullableInt()), typeof(int));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(int?))
                {
                    UnaryExpression constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableInt()),
                        typeof(int?));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(short))
                {
                    UnaryExpression constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableShort()),
                        typeof(short));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(short?))
                {
                    UnaryExpression constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableShort()),
                        typeof(short?));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(bool))
                {
                    UnaryExpression constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableBool()),
                        typeof(bool));
                    comparison = Expression.Equal(property, constant);
                }
                else if (property.Type == typeof(bool?))
                {
                    UnaryExpression constant = Expression.Convert(Expression.Constant(filter.Value?.ToNullableBool()),
                        typeof(bool?));
                    comparison = Expression.Equal(property, constant);
                }
                else
                {
                    ConstantExpression constant =
                        Expression.Constant(Convert.ToInt32(filter.Value, CultureInfo.InvariantCulture));
                    comparison = Expression.Equal(property, constant);
                }

                filterExpression = filterExpression is null ? comparison : Expression.And(filterExpression, comparison);
            }

            if (filterExpression is null)
            {
                return null;
            }

            // Create the lambda expression with the parameter and the filter expression
            filters = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
        }
        catch (Exception)
        {
            filters = null;
        }

        return filters;
    }

    private static int? ToNullableInt(this string s)
    {
        if (int.TryParse(s, out int i))
        {
            return i;
        }

        return null;
    }

    private static short? ToNullableShort(this string s)
    {
        if (short.TryParse(s, out short i))
        {
            return i;
        }

        return null;
    }

    private static bool? ToNullableBool(this string s)
    {
        if (bool.TryParse(s, out bool i))
        {
            return i;
        }

        return null;
    }
}

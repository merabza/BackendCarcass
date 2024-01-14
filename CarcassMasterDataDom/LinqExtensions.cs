using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CarcassMasterDataDom;

public static class LinqExtensions
{
    private static PropertyInfo GetPropertyInfo(Type objType, string name)
    {
        var properties = objType.GetProperties();
        var matchedProperty = properties.FirstOrDefault(p => p.Name == name);
        return matchedProperty ?? throw new ArgumentException("Invalid Argument", nameof(name));
    }

    private static LambdaExpression GetOrderExpression(Type objType, MemberInfo pi)
    {
        var paramExpr = Expression.Parameter(objType);
        var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
        var expr = Expression.Lambda(propAccess, paramExpr);
        return expr;
    }

    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name,
        bool ascending)
    {
        var propInfo = GetPropertyInfo(typeof(T), name);
        var expr = GetOrderExpression(typeof(T), propInfo);
        var methodName = ascending ? nameof(Enumerable.OrderBy) : nameof(Enumerable.OrderByDescending);
        var method = typeof(Enumerable).GetMethods()
                         .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2) ??
                     throw new Exception($"cannot find method {methodName}");
        var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
        var result = (IEnumerable<T>?)genericMethod.Invoke(null, [query, expr.Compile()]);
        if (result is not null)
            return result;

        throw new Exception("OrderBy result is not null");
    }

    public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string name, bool ascending)
    {
        //var propInfo = subObjType is null ? GetPropertyInfo(typeof(T), name) : GetPropertyInfo(subObjType, name);
        var propInfo = GetPropertyInfo(typeof(T), name);
        //var expr = subObjType is null ? GetOrderExpression(typeof(T), propInfo) : GetOrderExpression(subObjType, propInfo);
        var expr = GetOrderExpression(typeof(T), propInfo);
        var methodName = ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
        var method =
            typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2) ??
            throw new Exception($"cannot find method {methodName}");
        //var genericMethod = subObjType is null ? method.MakeGenericMethod(typeof(T), propInfo.PropertyType) : method.MakeGenericMethod(subObjType, propInfo.PropertyType);
        var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
        var result = (IQueryable<T>?)genericMethod.Invoke(null, [query, expr]);
        if (result is not null)
            return result;

        throw new Exception("OrderBy result is not null");
    }
}
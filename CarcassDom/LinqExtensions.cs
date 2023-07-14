using System.Linq.Expressions;
using System.Reflection;

namespace CarcassDom;

public static class LinqExtensions
{
    private static PropertyInfo GetPropertyInfo(Type objType, string name)
    {
        var properties = objType.GetProperties();
        var matchedProperty = properties.FirstOrDefault(p => p.Name == name);
        return matchedProperty ?? throw new ArgumentException("Invalid Argument", nameof(name));
    }

    private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
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
        var result = (IEnumerable<T>?)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        if (result is not null)
            return result;

        throw new Exception("OrderBy result is not null");
    }

    public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string name,
        bool ascending)
    {
        var propInfo = GetPropertyInfo(typeof(T), name);
        var expr = GetOrderExpression(typeof(T), propInfo);
        var methodName = ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
        var method = typeof(Queryable).GetMethods()
                         .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2) ??
                     throw new Exception($"cannot find method {methodName}");
        var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
        var result = (IQueryable<T>?)genericMethod.Invoke(null, new object[] { query, expr });
        if (result is not null)
            return result;

        throw new Exception("OrderBy result is not null");
    }
}
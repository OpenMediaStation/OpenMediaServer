using System.Linq.Expressions;

namespace OpenMediaServer.Helpers;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        
        var body = Expression.AndAlso(left.Body, right.Body);
        
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
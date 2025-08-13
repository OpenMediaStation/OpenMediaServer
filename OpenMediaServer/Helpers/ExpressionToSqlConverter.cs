using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace OpenMediaServer.Helpers;

public static class ExpressionToSqlConverter
{
    public static string ExpressionToSql(LambdaExpression? expression)
    {
        if (expression == null)
            return string.Empty;

        var body = expression.Body;

        var sb = new StringBuilder();
        sb.Append($" WHERE ");
        VisitExpression(body);

        return sb.ToString();

        void VisitExpression(Expression expr)
        {
            switch (expr)
            {
                case BinaryExpression binary:
                    sb.Append('(');
                    VisitExpression(binary.Left);
                    sb.Append(' ');
                    sb.Append(GetSqlOperator(binary.NodeType));
                    sb.Append(' ');
                    VisitExpression(binary.Right);
                    sb.Append(')');
                    break;

                case MemberExpression member:
                    if (member.Expression is ParameterExpression)
                    {
                        sb.Append(member.Member.Name);
                    }
                    else // z. B. captured variable
                    {
                        var value = GetValueFromExpression(member);
                        AppendSqlValue(value, sb);
                    }

                    break;

                case ConstantExpression constant:
                    AppendSqlValue(constant.Value, sb);
                    break;

                case UnaryExpression unary when unary.NodeType == ExpressionType.Convert:
                    VisitExpression(unary.Operand);
                    break;

                default:
                    var val = GetValueFromExpression(expr);
                    AppendSqlValue(val, sb);
                    break;
            }
        }

        static string GetSqlOperator(ExpressionType nodeType)
        {
            return nodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                ExpressionType.GreaterThan => ">",
                ExpressionType.LessThan => "<",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThanOrEqual => "<=",
                _ => throw new NotSupportedException($"Operator '{nodeType}' is not supported."),
            };
        }

        static object? GetValueFromExpression(Expression expr)
        {
            try
            {
                var lambda = Expression.Lambda(expr);
                var compiled = lambda.Compile();
                return compiled.DynamicInvoke();
            }
            catch
            {
                return null;
            }
        }
    }

    public static void AppendSqlValue(object? value, StringBuilder sb)
    {
        switch (value)
        {
            case null:
                sb.Append("NULL");
                break;
            case string:
            case Guid:
                sb.Append('\'');
                sb.Append(value.ToString()?.Replace("'", "''"));
                sb.Append('\'');
                break;
            case bool b:
                sb.Append(b ? "TRUE" : "FALSE");
                break;
            case DateOnly:
                var dateOnly = value is DateOnly only ? only : default;
                
                var isoString = dateOnly.ToString("yyyy-MM-dd");
                
                sb.Append($"\'{isoString}\'");
                break;            
            case DateTime:
                var dateTime = value is DateTime dt ? dt : default;
                var isoStringDateTime = dateTime.ToString("yyyy-MM-dd");
                
                sb.Append($"\'{isoStringDateTime}\'");
                break;
            default:
                sb.Append(value);
                break;
        }
    }
    
    public static string GetTableName(Type type)
    {
        if (type.BaseType != null && type.BaseType.Assembly == Assembly.GetExecutingAssembly())
            type = type.BaseType;
        var className = type.Name;
        var tableName = "t_" + className;
        return tableName.ToLower();
    }
}
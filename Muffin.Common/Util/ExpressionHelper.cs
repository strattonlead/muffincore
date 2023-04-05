using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Muffin.Common.Util
{
    public static class ExpressionHelper
    {
        // https://stackoverflow.com/a/457328/633945
        public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left, right), parameter);
        }

        public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.Or(left, right), parameter);
        }

        // https://stackoverflow.com/a/457328/633945
        private class ReplaceExpressionVisitor
            : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }

        public static Dictionary<string, object> GetMethodParameters(this MethodCallExpression body)
        {
            if (body == null)
            {
                throw new ArgumentException("the expression body must be of type MethodCallExpression");
            }

            var result = new Dictionary<string, object>();
            for (var i = 0; i < body.Arguments.Count; i++)
            {
                var argument = body.Arguments[i];
                var parameter = body.Method.GetParameters()[i];
                var name = parameter.Name;

                var value = Expression.Lambda(argument).Compile().DynamicInvoke();
                result.Add(name, value);
            }
            return result;
        }
    }
}

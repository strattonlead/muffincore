using System;
using System.Linq;
using System.Linq.Expressions;

namespace Muffin.Common.Api.WebSockets
{
    public static class ApiRequestHelper
    {
        public static ApiRequest FromExpression<T>(Expression<Action<T>> expression)
        {
            return FromExpression(expression, null);
        }

        public static ApiRequest FromExpression<T>(Expression<Action<T>> expression, string requestId)
        {
            return FromExpression(expression, null, requestId);
        }

        public static ApiRequest FromExpression<T>(Expression<Action<T>> expression, string action, string requestId)
        {
            var body = (MethodCallExpression)expression.Body;
            var request = new ApiRequest(body.Method.Name);

            var controllerAttribute = (ControllerAttribute)typeof(T)
                .GetCustomAttributes(typeof(ControllerAttribute), true)
                .FirstOrDefault();

            if (action != null)
            {
                request.Action = action;
            }
            request.Controller = controllerAttribute?.Name ?? "";
            request.RequestId = requestId ?? "";

            for (var i = 0; i < body.Arguments.Count; i++)
            {
                var argument = body.Arguments[i];
                var member = _resolveMemberExpression(argument);

                var parameter = body.Method.GetParameters()[i];
                var name = parameter.Name;

                var value = _getValue(member);
                request.SetParam(name, value);
            }
            return request;
        }

        private static MemberExpression _resolveMemberExpression(Expression expression)
        {

            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            else if (expression is UnaryExpression)
            {
                return (MemberExpression)((UnaryExpression)expression).Operand;
            }
            else
            {
                throw new NotSupportedException(expression.ToString());
            }
        }

        private static object _getValue(MemberExpression exp)
        {
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value)
                        .GetType()
                        .GetField(exp.Member.Name)
                        .GetValue(((ConstantExpression)exp.Expression).Value);
            }
            else if (exp.Expression is MemberExpression)
            {
                return _getValue((MemberExpression)exp.Expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class ControllerAttribute : Attribute
    {
        public string Name { get; set; }

        public ControllerAttribute() { }
        public ControllerAttribute(string name) { Name = name; }
    }
}

using Muffin.Common.Api.WebSockets;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Muffin.WebSockets.Server.Extensions
{
    public static class ApiRequestExtensions
    {
        public static ApiRequest DynamicMethodCall<T>(Expression<Action<T>> expression)
        {

            var body = (MethodCallExpression)expression.Body;
            var controllerAttribute = (ControllerAttribute)typeof(T)
                .GetCustomAttributes(typeof(ControllerAttribute))
                .FirstOrDefault();

            var controller = controllerAttribute?.Name;
            var request = new ApiRequest(body.Method.Name, controller);
            for (var i = 0; i < body.Arguments.Count; i++)
            {
                var argument = body.Arguments[i];

                if (argument is ConstantExpression)
                {

                }
                var member = ResolveMemberExpression(argument);

                var parameter = body.Method.GetParameters()[i];
                var name = parameter.Name;

                var value = GetValueFromMemberExpression(member);
                request.SetParam(name, value);
            }
            return request;
        }

        public static MemberExpression ResolveMemberExpression(Expression expression)
        {

            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            else if (expression is UnaryExpression)
            {
                // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
                return (MemberExpression)((UnaryExpression)expression).Operand;
            }
            else
            {
                throw new NotSupportedException(expression.ToString());
            }
        }

        public static object GetValueFromMemberExpression(MemberExpression exp)
        {
            // expression is ConstantExpression or FieldExpression
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value)
                        .GetType()
                        .GetField(exp.Member.Name)
                        .GetValue(((ConstantExpression)exp.Expression).Value);
            }
            else if (exp.Expression is MemberExpression)
            {
                return GetValueFromMemberExpression((MemberExpression)exp.Expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

using Muffin.Common.Api.WebSockets;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Client
{
    public static class ApiHelper
    {
        public static async Task SendAsync(this WebSocketClient client, ApiRequest obj)
        {
            var data = JsonConvert.SerializeObject(obj);
            await Task.Run(() => client.WebSocket.Send(data));
        }

        public static async Task SendAsync<T>(this WebSocketClient client, Expression<Action<T>> expression)
        {
            await client.SendAsync(DynamicMethodCall<T>(expression, null));
        }

        public static async Task SendAsync<T>(this WebSocketClient client, Expression<Action<T>> expression, object requestId)
        {
            await client.SendAsync(DynamicMethodCall<T>(expression, requestId?.ToString()));
        }

        public static ApiRequest DynamicMethodCall<T>(Expression<Action<T>> expression, string requestId)
        {
            var body = (MethodCallExpression)expression.Body;

            var request = new ApiRequest(body.Method.Name);
            request.RequestId = requestId;
            var controllerAttr = typeof(T).GetCustomAttribute<ControllerAttribute>();
            if (controllerAttr != null && !string.IsNullOrWhiteSpace(controllerAttr.Name))
            {
                if (controllerAttr.Name.EndsWith("Controller"))
                {
                    request.Controller = controllerAttr.Name.Replace("Controller", "");
                }
                else
                {
                    request.Controller = controllerAttr.Name;
                }
            }

            for (var i = 0; i < body.Arguments.Count; i++)
            {
                var argument = body.Arguments[i];

                if (argument is ConstantExpression)
                {

                }
                var member = ResolveMemberExpression(argument);

                var parameter = body.Method.GetParameters()[i];
                var name = parameter.Name;

                var value = GetValue(member);
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

        private static object GetValue(MemberExpression exp)
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
                return GetValue((MemberExpression)exp.Expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

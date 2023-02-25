using Muffin.Common.Api.WebSockets;
using Newtonsoft.Json;
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

        public static string Serialize<T>(T obj, string action = "", string controller = "", string requestId = "")
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new FormatNumbersAsTextConverter() }
            };
            if (obj is ApiRequest && obj != null)
            {
                var apiRequest = (ApiRequest)(object)obj;
                if (!string.IsNullOrWhiteSpace(action))
                {
                    apiRequest.Action = action;
                }
                if (!string.IsNullOrWhiteSpace(controller))
                {
                    apiRequest.Controller = controller;
                }
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    apiRequest.RequestId = requestId;
                }
                return JsonConvert.SerializeObject(apiRequest, settings);
            }
            var response = new ApiRequest(action, controller);
            response.RequestId = requestId;
            response.SetParam("model", obj);
            return JsonConvert.SerializeObject(response, settings);
        }
    }
    internal sealed class FormatNumbersAsTextConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;
        public override bool CanConvert(Type type) => type == typeof(long) || type == typeof(long?);

        public override void WriteJson(
            JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(long))
                {
                    long number = (long)value;
                    writer.WriteValue(number.ToString());
                }
                else if (value.GetType() == typeof(long?))
                {
                    long? number = (long?)value;
                    writer.WriteValue(number.Value.ToString());
                }
            }
            else
            {
                writer.WriteValue((string)null);
            }
        }

        public override object ReadJson(
            JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}

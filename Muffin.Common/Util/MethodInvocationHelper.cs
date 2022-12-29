using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Muffin.Common.Util
{
    public static class MethodInvocationHelper
    {
        public static PossibleInvocations GetPossibleInvocations<T>(Expression<Func<T, Action>> methodSelector, Dictionary<string, object> input)
        {
            var unaryExpression = (UnaryExpression)methodSelector.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Arguments.Last();
            var methodInfo = (MemberInfo)methodInfoExpression.Value;
            return GetPossibleInvocations(typeof(T), methodInfo?.Name, input);
        }

        public static PossibleInvocations GetPossibleInvocations(Type type, string methodName, Dictionary<string, object> input)
        {
            return new PossibleInvocations(type, methodName, input);
        }

        public static PossibleInvocation GetPossibleInvocation(MethodInfo methodInfo, Dictionary<string, object> input)
        {
            var parameterInfos = methodInfo.GetParameters();
            var methodParameters = new List<MethodParameter>();

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];

                if (input.TryGetValue(parameterInfo.Name, out object value))
                {
                    if (value.GetType() == parameterInfo.ParameterType)
                    {
                        methodParameters.Add(new MethodParameter(parameterInfo, value));
                    }
                    else
                    {
                        var jsonValue = JsonConvert.SerializeObject(value);
                        var typeSafeValue = JsonConvert.DeserializeObject(jsonValue, parameterInfo.ParameterType/*, DeserializableInterfaceJsonHelper.GetInterfaceJsonSerializerSettings()*/);
                        methodParameters.Add(new MethodParameter(parameterInfo, typeSafeValue));
                    }
                }
            }

            var match = MethodMatch.NoMatch;
            if (parameterInfos.Length == methodParameters.Count)
            {
                match = MethodMatch.ExactMatch;
                if (parameterInfos.Length < input.Count)
                {
                    match = MethodMatch.MoreParametersProvidedMatch;
                }
            }

            return new PossibleInvocation(new MethodInvocation(methodInfo, methodParameters), match);
        }

        public enum MethodMatch
        {
            ExactMatch = 1,
            MoreParametersProvidedMatch = 2,
            NoMatch = 3,
        }

        public class PossibleInvocation
        {
            public bool IsAmbingous { get; set; }
            public MethodMatch Match { get; set; }
            public MethodInvocation MethodInvocation { get; set; }

            public PossibleInvocation(MethodInvocation methodInvocation, MethodMatch match)
            {
                MethodInvocation = methodInvocation;
                Match = match;
            }
        }

        public class PossibleInvocations
        {
            #region Properties

            private PossibleInvocation[] MatchedInvocations;

            #endregion

            #region Constructor

            public PossibleInvocations(Type type, string methodName, Dictionary<string, object> parameters)
            {
                var methodInfos = type
                    .GetMethods()
                    .Where(x => x.Name == methodName)
                    .ToArray();

                MatchedInvocations = methodInfos
                    .Select(x => MethodInvocationHelper.GetPossibleInvocation(x, parameters))
                    .ToArray();

                if (MatchedInvocations.Count(x => x.Match == MethodMatch.ExactMatch) >= 2)
                {
                    foreach (var item in MatchedInvocations.Where(x => x.Match == MethodMatch.ExactMatch))
                    {
                        item.IsAmbingous = true;
                    }
                }
            }

            #endregion

            #region Actions

            public MethodInvocation GetMostLikelyInvocation()
            {
                var result = MatchedInvocations.OrderBy(x => x.Match).FirstOrDefault();
                if (result?.IsAmbingous ?? false)
                {
                    throw new Exception($"Method Invocation is ambingous {result.MethodInvocation.MethodInfo.Name}");
                }
                return result?.MethodInvocation;
            }

            public object InvokeMostLikely(object obj)
            {
                return GetMostLikelyInvocation()?.Invoke(obj);
            }

            #endregion
        }

        public class MethodParameter
        {
            public ParameterInfo ParameterInfo { get; set; }
            public string Name { get { return ParameterInfo.Name; } }
            public object Value { get; set; }

            public MethodParameter(ParameterInfo parameterInfo, object value)
            {
                ParameterInfo = parameterInfo;
                Value = value;
            }
        }

        public class MethodInvocation
        {
            #region properties

            public readonly MethodInfo MethodInfo;
            public readonly MethodParameter[] MethodParameters;

            #endregion

            #region Constructor

            public MethodInvocation(MethodInfo methodInfo, IEnumerable<MethodParameter> methodParameters)
            {
                MethodInfo = methodInfo;
                MethodParameters = methodParameters?.ToArray();
            }

            #endregion

            #region Helper

            public object[] GetParameters()
            {
                if (MethodParameters == null)
                {
                    return null;
                }
                return MethodParameters.Select(x => x.Value).ToArray();
            }

            public object Invoke(object obj)
            {
                return MethodInfo?.Invoke(obj, GetParameters());
            }

            #endregion
        }


    }
}

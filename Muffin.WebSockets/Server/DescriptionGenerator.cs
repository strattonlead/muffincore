using Muffin.Common.Api.WebSockets;
using Muffin.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server
{
    public class DescriptionGenerator
    {
        // Controller / Action / Parameter struktur und namen
        public MethodDescription[] GetInterfaceDescription<T>(int includeParentTypeLevels = 0, ClassDescriptionOptions options = null)
        {
            var type = typeof(T);
            var controllerAttribute = (ControllerAttribute)type.GetCustomAttributes(typeof(ControllerAttribute), true).FirstOrDefault();
            string controllerName = null;
            if (controllerAttribute != null)
            {
                controllerName = controllerAttribute.Name;
            }

            var methods = new List<_M>();
            methods.AddRange(type.GetMethods().Select(x => new _M() { Type = type, MethodInfo = x }));
            for (var i = 0; i < includeParentTypeLevels; i++)
            {
                if (!type.GetInterfaces()?.Any() ?? false)
                {
                    break;
                }

                foreach (var baseType in type.GetInterfaces())
                {
                    methods.AddRange(baseType.GetMethods().Select(x => new _M() { Type = baseType, MethodInfo = x }));
                }
            }

            return methods.Select(x => new MethodDescription(x.MethodInfo, x.Type, controllerName, options)).ToArray();
        }
    }

    public class _M
    {
        public Type Type { get; set; }
        public MethodInfo MethodInfo { get; set; }
    }

    public class ReturnTypeAttribute : Attribute
    {
        public Type ReturnType { get; set; }

        public ReturnTypeAttribute(Type returnType)
        {
            ReturnType = returnType;
        }
    }

    public class MethodDescription
    {
        public Type InterfaceType { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get { return MethodInfo.Name; } }
        public ParameterDescription[] Parameters { get; set; }
        public ReturnTypeDescription ReturnType { get; set; }
        public bool HasReturnType { get { return ReturnType != null; } }

        public MethodDescription(MethodInfo methodInfo, Type interfaceType, string controllerName, ClassDescriptionOptions options)
        {
            InterfaceType = interfaceType;
            ControllerName = controllerName;
            MethodInfo = methodInfo;

            Parameters = methodInfo.GetParameters()
                .Select(x => new ParameterDescription(x, options))
                .ToArray();

            var returnTypeAttribute = (ReturnTypeAttribute)MethodInfo.GetCustomAttributes(typeof(ReturnTypeAttribute), true).FirstOrDefault();
            if (returnTypeAttribute != null && returnTypeAttribute.ReturnType != null)
            {
                if (returnTypeAttribute.ReturnType == typeof(void) || returnTypeAttribute.ReturnType == typeof(Task))
                {
                    ReturnType = null;
                }
                else
                {
                    ReturnType = new ReturnTypeDescription(returnTypeAttribute.ReturnType, options);
                }
            }
            else
            {
                if (methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == typeof(Task))
                {
                    ReturnType = null;
                }
                else
                {
                    ReturnType = new ReturnTypeDescription(methodInfo.ReturnType, options);
                }
            }
        }
    }

    public abstract class TypeDescription
    {
        public abstract Type ElementOrBaseType { get; }
        public string TypeDescriptionJson { get; protected set; }
    }

    public class ReturnTypeDescription : TypeDescription
    {
        public override Type ElementOrBaseType
        {
            get
            {
                if (typeof(IEnumerable).IsAssignableFrom(ReturnType))
                {
                    return ReturnType.GetElementType();
                }
                if (ReturnType.IsGenericType && ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return ReturnType.GetGenericArguments()[0];
                }
                return ReturnType;
            }
        }
        public Type ReturnType { get; set; }

        public ReturnTypeDescription(Type type, ClassDescriptionOptions options)
        {
            ReturnType = type;

            try
            {
                var instance = ClassHelper.GetClassDescription(ElementOrBaseType, options);
                TypeDescriptionJson = JsonConvert.SerializeObject(instance, Formatting.Indented);
            }
            catch { }
        }
    }

    public class ParameterDescription : TypeDescription
    {
        public string Name { get { return ParameterInfo.Name; } }
        public Type ParameterType { get { return ParameterInfo.ParameterType; } }
        public override Type ElementOrBaseType
        {
            get
            {
                if (typeof(IEnumerable).IsAssignableFrom(ParameterInfo.ParameterType))
                {
                    return ParameterInfo.ParameterType.GetElementType();
                }
                return ParameterInfo.ParameterType;
            }
        }
        public ParameterInfo ParameterInfo { get; set; }

        public ParameterDescription(ParameterInfo parameterInfo, ClassDescriptionOptions options)
        {
            ParameterInfo = parameterInfo;

            try
            {
                var instance = ClassHelper.GetClassDescription(ElementOrBaseType, options);
                TypeDescriptionJson = JsonConvert.SerializeObject(instance, Formatting.Indented);
            }
            catch { }
        }
    }
}

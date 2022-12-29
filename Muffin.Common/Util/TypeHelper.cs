using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Muffin.Common.Util
{
    public static class TypeHelper
    {
        #region Attribute Helper

        public static T GetAttribute<T>(this Type type, bool inherit)
            where T : Attribute
        {
            return (T)type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static Attribute GetAttribute(this Type self, Type type, bool inherit)
        {
            var properties = self.GetCustomAttributes(type, inherit);
            if (!properties.Any())
                return null;
            return (Attribute)properties.FirstOrDefault();
        }

        public static T GetAttribute<T>(this PropertyInfo propertyInfo, bool inherit)
            where T : Attribute
        {
            return propertyInfo.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

        public static Attribute GetAttribute(this PropertyInfo propertyInfo, Type type, bool inherit)
        {
            return (Attribute)propertyInfo.GetCustomAttributes(type, inherit).FirstOrDefault();
        }

        public static bool HasAttribute<T>(this Type type)
            where T : Attribute
        {
            return type.GetAttribute<T>(true) != null;
        }

        public static bool HasAttribute(this Type self, Type type)
        {
            return self.GetAttribute(type, true) != null;
        }

        public static bool HasAttribute<T>(this PropertyInfo propertyInfo)
            where T : Attribute
        {
            return propertyInfo.GetAttribute<T>(true) != null;
        }

        public static bool HasAttribute(this PropertyInfo propertyInfo, Type type)
        {
            return propertyInfo.GetAttribute(type, true) != null;
        }

        public static IEnumerable<Type> GetTypesWithAttribute<T>()
            where T : Attribute
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypesWithAttribute<T>()).ToArray();
        }

        public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly)
            where T : Attribute
        {
            foreach (Type type in assembly.GetTypes())
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                    yield return type;
        }

        public static IEnumerable<MethodInfo> GetMethodInfosWithAttribute<T>(this Type type)
            where T : Attribute
        {
            foreach (var methodInfo in type.GetMethods())
                if (methodInfo.GetCustomAttribute<T>() != null)
                    yield return methodInfo;
        }

        #endregion

        #region Method Helper

        #endregion

        #region Constants Helper

        public static FieldInfo[] GetConstants<T>()
        {
            return typeof(T).GetConstants();
        }

        public static FieldInfo[] GetConstants<T>(this T obj)
        {
            return typeof(T).GetConstants();
        }

        public static FieldInfo[] GetConstants(this Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
         BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToArray();
        }

        public static FieldInfo GetConstant<T>(this T obj, string name)
        {
            return typeof(T).GetConstant(name);
        }

        public static FieldInfo GetConstant(this Type type, string name)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).FirstOrDefault(fi => fi.Name.Equals(name) && fi.IsLiteral && !fi.IsInitOnly);
        }

        public static object GetConstantValue<T>(this T obj, string name)
            where T : class
        {
            var fieldInfo = obj.GetConstant(name);
            if (fieldInfo == null)
                return null;
            return fieldInfo.GetValue(null);
        }

        public static object GetConstantValue(this Type type, string name)
        {
            var fieldInfo = type.GetConstant(name);
            if (fieldInfo == null)
                return null;
            return fieldInfo.GetValue(null);
        }

        public static object[] GetConstantValues(this Type type)
        {
            return GetConstants(type).Select(x => x.GetValue(null)).ToArray();
        }

        #endregion

        #region Convert

        public static TOut? ConvertNullable<TIn, TOut>(this TIn? input)
            where TIn : struct
            where TOut : struct
        {
            if (!input.HasValue)
            {
                return default;
            }

            return (TOut)Convert.ChangeType(input.Value, typeof(TOut));
        }

        #endregion
    }

}

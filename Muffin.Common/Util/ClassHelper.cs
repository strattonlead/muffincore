using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Muffin.Common.Util
{
    public static class ClassHelper
    {
        public static Dictionary<string, object> GetClassDescription<T>()
        {
            return GetClassDescription<T>(new ClassDescriptionOptions());
        }

        public static Dictionary<string, object> GetClassDescription<T>(ClassDescriptionOptions options)
        {
            return GetClassDescription(typeof(T), options);
        }

        public static Dictionary<string, object> GetClassDescription(Type type, ClassDescriptionOptions options)
        {
            if (options == null)
            {
                options = new ClassDescriptionOptions();
            }

            var result = new Dictionary<string, object>();
            _getClassDescription(result, type, options);
            return result;
        }

        private static readonly Type[] _baseTypes = new Type[] {
            typeof(object),
            typeof(string),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(TimeSpan),
            typeof(TimeSpan?),
            typeof(byte?),
            typeof(short?),
            typeof(int?),
            typeof(long?),
            typeof(decimal?),
            typeof(double?)
        };

        private static string GetName(this PropertyInfo propertyInfo)
        {
            var jsonPropertyAttribute = (JsonPropertyAttribute)propertyInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault();
            if (jsonPropertyAttribute != null && !string.IsNullOrWhiteSpace(jsonPropertyAttribute.PropertyName))
            {
                return jsonPropertyAttribute.PropertyName;
            }
            return propertyInfo.Name;
        }

        private static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                | BindingFlags.Public | BindingFlags.Instance);
        }

        private static void _getClassDescription(Dictionary<string, object> parent, Type type, ClassDescriptionOptions options)
        {
            //var properties = type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            var properties = type.GetPublicProperties();
            if (options.IgnoreJsonIgnoreAttributes)
            {
                properties = properties
                    .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                    .ToArray();
            }

            if (properties.Length == 0)
            {
                return;
            }



            foreach (var property in properties)
            {
                // Wir sind am Ende angekommen, also flat rein legen
                if (property.PropertyType.IsPrimitive || _baseTypes.Contains(property.PropertyType))
                {
                    if (Nullable.GetUnderlyingType(property.PropertyType) == null)
                    {
                        parent[property.GetName()] = property.PropertyType.Name;
                    }
                    else
                    {
                        parent[property.GetName()] = $"{Nullable.GetUnderlyingType(property.PropertyType).Name} (nullable)";
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    var enumValues = EnumHelper.EnumList(property.PropertyType);
                    parent[property.GetName()] = $"{property.PropertyType.Name} -> {string.Join(",", enumValues.Select(x => $"{x}={(int)x}"))}";
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        try
                        {
                            var keyType = property.PropertyType.GetGenericArguments()[0];
                            var valueType = property.PropertyType.GetGenericArguments()[1];

                            var dict = new Dictionary<string, object>();
                            parent[property.GetName()] = dict;
                            if (valueType.IsPrimitive || _baseTypes.Contains(valueType))
                            {
                                dict[keyType.Name] = valueType.Name;
                            }
                            else
                            {
                                var _parent = new Dictionary<string, object>();
                                dict[keyType.Name] = _parent;
                                _getClassDescription(_parent, valueType, options);
                            }
                        }
                        catch { parent[property.GetName()] = "Error"; }
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        var valueType = property.PropertyType.GetElementType();
                        var array = Array.CreateInstance(typeof(Dictionary<string, object>), 1);

                        parent[property.GetName()] = array;
                        var _parent = new Dictionary<string, object>();
                        array.SetValue(_parent, 0);
                        _getClassDescription(_parent, valueType, options);
                    }
                    else
                    {
                        parent[property.GetName()] = property.PropertyType.Name;
                    }
                }
                else
                {
                    var dict = new Dictionary<string, object>();
                    parent[property.GetName()] = dict;
                    _getClassDescription(dict, property.PropertyType, options);
                }
            }
        }
    }

    public class ClassDescriptionOptions
    {
        public bool IgnoreJsonIgnoreAttributes { get; set; }
    }
}

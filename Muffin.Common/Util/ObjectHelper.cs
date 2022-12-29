using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace Muffin.Common.Util
{
    public static class ObjectHelper
    {
        public static bool ObjectChanged<T>(T source, T objectToCheck)
        {
            var propertiesToCheck = typeof(T)
                .GetProperties()
                .Where(x => x.HasAttribute<PropertyChangedAttribute>())
                .ToArray();

            foreach (var property in propertiesToCheck)
            {
                var sourceValue = property.GetValue(source);
                var objectValue = property.GetValue(objectToCheck);
                if (sourceValue == null && objectValue == null)
                    continue;
                if ((sourceValue == null && objectValue != null)
                    || (objectValue == null && sourceValue != null))
                    return true;
                if (!sourceValue.Equals(objectValue))
                    return true;
            }
            return false;
        }

        public static dynamic AggregateToDynamic<TValue>(this IDictionary<string, TValue> dict)
        {
            dynamic eo = dict.Select(x => new KeyValuePair<string, object>(x.Key, x.Value))
                .Aggregate(new ExpandoObject() as IDictionary<string, Object>,
                                        (a, p) => { a.Add(p); return a; });
            return eo;
        }

        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
            {
                expando.Add(property.Name, property.GetValue(value));
            }

            return expando as ExpandoObject;
        }

        public static Dictionary<string, object> ToDictionary(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is Dictionary<string, object>)
            {
                return (Dictionary<string, object>)obj;
            }

            var result = new Dictionary<string, object>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                var value = property.GetValue(obj);
                if (value == null)
                {
                    result[property.Name] = value;
                }
                else if (value is Dictionary<string, object>)
                {
                    result[property.Name] = value;
                }
                else if (value != null && value.GetType().IsPrimitive)
                {
                    result[property.Name] = value;
                }
                else
                {
                    result[property.Name] = ToDictionary(value);
                }
            }

            return result;
        }

        public static T DeepClone<T>(T src)
            where T : class
        {
            var copy = JsonConvert.SerializeObject(src);
            return JsonConvert.DeserializeObject<T>(copy);
        }

        public static T DeepClone<T>(T src, Action<T> postProcessing)
            where T : class
        {
            var copy = JsonConvert.SerializeObject(src);
            var result = JsonConvert.DeserializeObject<T>(copy);
            postProcessing?.Invoke(result);
            return result;
        }

        public static T DeepClone<T>(T src, Action<T> postProcessing, JsonSerializerSettings settings)
            where T : class
        {
            var copy = JsonConvert.SerializeObject(src);
            var result = JsonConvert.DeserializeObject<T>(copy, settings);
            postProcessing?.Invoke(result);
            return result;
        }
    }

    public class PropertyChangedAttribute : Attribute { }

    public interface ICloneable<T> : ICloneable
    {
        new T Clone();
    }
}

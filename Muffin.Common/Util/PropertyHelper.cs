using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Muffin.Common.Util
{
    public static class PropertyHelper
    {

        public static void CopyProperties(object source, object destination)
        {
            CopyProperties(source, destination, (PropertyCopyOptions)null);
        }

        public static void CopyProperties(object source, object destination, PropertyCopyOptions options)
        {
            CopyProperties(source, destination, options, null);
        }

        public static void CopyProperties(object source, object destination, params string[] propertiesToIgnore)
        {
            CopyProperties(source, destination, (PropertyCopyOptions)null, propertiesToIgnore);
        }

        public static void CopyProperties(object source, object destination, PropertyCopyOptions options, params string[] propertiesToIgnore)
        {
            if (source == null)
                return;
            if (destination == null)
                return;

            var propertyPairs = _getPropertyPairs(source, destination, propertiesToIgnore, options);

            var destinationArrays = propertyPairs.Where(x => x.IsDestinationArray)
                .GroupBy(x => x.DestinationProperty.Name)
                .Select(x => new
                {
                    x.First().DestinationProperty,
                    Length = x.Count()
                })
                .ToArray();

            foreach (var destinationArray in destinationArrays)
            {
                var array = (Array)destinationArray.DestinationProperty.GetValue(destination);
                if (array == null)
                {
                    array = Array.CreateInstance(destinationArray.DestinationProperty.PropertyType.GetElementType(), destinationArray.Length);
                    destinationArray.DestinationProperty.SetValue(destination, array);
                }
                else if (array.Length != destinationArray.Length)
                {
                    array = Array.CreateInstance(destinationArray.DestinationProperty.PropertyType.GetElementType(), destinationArray.Length);
                    destinationArray.DestinationProperty.SetValue(destination, array);
                }
            }

            foreach (var propertyPair in propertyPairs)
                propertyPair.TransferValue(source, destination);
        }

        // TODO source und destination Paare Cachen
        private static Dictionary<string, PropertyMatch[]> _propertyPairCache = new Dictionary<string, PropertyMatch[]>();
        private static object _accessLock = new object();
        private static PropertyMatch[] _getPropertyPairs(object source, object destination, string[] propertiesToIgnore, PropertyCopyOptions options)
        {
            lock (_accessLock)
            {
                string combinationKey;
                if (propertiesToIgnore == null)
                {
                    combinationKey = $"{source.GetType().AssemblyQualifiedName} {destination.GetType().AssemblyQualifiedName}";
                }
                else
                {
                    combinationKey = $"{source.GetType().AssemblyQualifiedName} {destination.GetType().AssemblyQualifiedName} {string.Join(";", propertiesToIgnore)}";
                }

                if (_propertyPairCache.ContainsKey(combinationKey))
                {
                    return _propertyPairCache[combinationKey];
                }

                if (propertiesToIgnore == null)
                {
                    propertiesToIgnore = new string[0];
                }

                var sourceProperties = source.GetType().GetProperties().Where(x => !propertiesToIgnore.Contains(x.Name)).ToArray();
                var destinationProperties = destination.GetType().GetProperties().Where(x => !propertiesToIgnore.Contains(x.Name)).ToArray();

                var mappedSourceProperties = sourceProperties.Where(x => x.HasAttribute<PropertyMapAttribute>()).ToArray();
                var mappedDestinationProperties = destinationProperties.Where(x => x.HasAttribute<PropertyMapAttribute>()).ToArray();

                var directSourceProperties = sourceProperties.Except(mappedSourceProperties)
                    .GroupBy(x => x.Name)
                    .Select(x => x.First())
                    .ToDictionary(x => x.Name, x => x);

                var directDestinationProperties = destinationProperties.Except(mappedDestinationProperties)
                    .GroupBy(x => x.Name)
                    .Select(x => x.First())
                    .ToDictionary(x => x.Name, x => x);

                var propertyMatches = new List<PropertyMatch>();

                foreach (var directSourcePropertyPair in directSourceProperties)
                {
                    var directSourceProperty = directSourcePropertyPair.Value;
                    if (!directDestinationProperties.ContainsKey(directSourcePropertyPair.Key))
                        continue;
                    var directDestinationProperty = directDestinationProperties[directSourcePropertyPair.Key];

                    var match = new PropertyMatch(options)
                    {
                        SourceProperty = directSourceProperty,
                        DestinationProperty = directDestinationProperty,
                    };
                    propertyMatches.Add(match);
                }

                if (mappedSourceProperties.Any())
                    foreach (var mappedSourceProperty in mappedSourceProperties)
                    {
                        var propertyMap = mappedSourceProperty.GetAttribute<PropertyMapAttribute>(true);
                        if (mappedSourceProperty.PropertyType.IsArray && propertyMap.ArraySource != null)
                        {
                            for (var i = 0; i < propertyMap.ArraySource.Length; i++)
                            {
                                var match = new PropertyMatch(options)
                                {
                                    IsSourceArray = true,
                                    ArrayIndex = i,
                                    SourceProperty = mappedSourceProperty,
                                    DestinationProperty = propertyMap.GetPropertyInfo(destination, i),
                                    PropertyMap = propertyMap
                                };
                                propertyMatches.Add(match);
                            }
                        }
                        else
                        {
                            var match = new PropertyMatch(options)
                            {
                                SourceProperty = mappedSourceProperty,
                                DestinationProperty = propertyMap.GetPropertyInfo(destination),
                                PropertyMap = propertyMap
                            };
                            propertyMatches.Add(match);
                        }
                    }

                if (mappedDestinationProperties.Any())
                    foreach (var mappedDestinationProperty in mappedDestinationProperties)
                    {
                        var propertyMap = mappedDestinationProperty.GetAttribute<PropertyMapAttribute>(true);
                        if (mappedDestinationProperty.PropertyType.IsArray && propertyMap.ArraySource != null)
                        {
                            for (var i = 0; i < propertyMap.ArraySource.Length; i++)
                            {
                                var match = new PropertyMatch(options)
                                {
                                    IsDestinationArray = true,
                                    ArrayIndex = i,
                                    SourceProperty = propertyMap.GetPropertyInfo(source, i),
                                    DestinationProperty = mappedDestinationProperty,
                                    PropertyMap = propertyMap
                                };
                                propertyMatches.Add(match);
                            }
                        }
                        else
                        {
                            var match = new PropertyMatch(options)
                            {
                                SourceProperty = propertyMap.GetPropertyInfo(source),
                                DestinationProperty = mappedDestinationProperty,
                                PropertyMap = propertyMap
                            };
                            propertyMatches.Add(match);
                        }
                    }

                var result = propertyMatches.ToArray();
                _propertyPairCache.Add(combinationKey, result);
                return result;
            }
        }

        public static void SetStringNullValuesToEmpty(object source)
        {
            if (source == null)
                return;

            var properties = source.GetType()
                .GetProperties()
                .Where(x => x.PropertyType == typeof(string))
                .ToArray();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source);
                if (value == null)
                    prop.SetValue(source, "");
            }
        }

        public static Dictionary<string, string> ToDictionary<T>(T obj)
            where T : class
        {
            if (obj == null)
                return new Dictionary<string, string>();

            var properties = obj.GetType().GetProperties();
            var dict = new Dictionary<string, string>();
            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                dict.Add(property.Name, value != null ? value.ToString() : "");
            }
            return dict;
        }

        //public static void EncryptValues()
        //{


        //    AES256.EncryptText("", "0DA50E28-69B1-4C13-9020-01AC55DF81B8", new UTF8Encoding().GetBytes("76133FF7-A892-4536-9590-1052FA6A7321"));
        //    //       Key = "0DA50E28-69B1-4C13-9020-01AC55DF81B8",
        //    //Salt = new UTF8Encoding().GetBytes("76133FF7-A892-4536-9590-1052FA6A7321"),
        //    //Mode = CipherMode.CBC,
        //    //Padding = PaddingMode.PKCS7,
        //    //StringEncoding = new UTF8Encoding()
        //}

        //public static void DecryptValues()
        //{

        //}

        public static object GetPropertyValue(object src, string propName)
        {
            if (src == null)
                return null;
            if (propName == null)
                throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains("."))
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var type = src.GetType();
                var prop = type.GetProperties().FirstOrDefault(x => string.Equals(propName, x.Name) && x.DeclaringType == type);

                if (prop == null)
                    prop = type.GetProperties().FirstOrDefault(x => string.Equals(propName, x.Name));

                if (prop != null)
                    return prop.GetValue(src, null);

                if (propName.Contains("(") || propName.Contains(")"))
                    propName.Replace("(", "").Replace(")", "");
                var method = src.GetType().GetMethod(propName);
                if (method != null)
                    return method.Invoke(src, null); // TODO parameter noch parsen falls vorhanden
            }
            return null;
        }

        public static void SetPropertyValue<T, TParam>(this T target, object value, Expression<Func<T, TParam>> memberLamda)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    if (property.CanWrite)
                        property.SetValue(target, value, null);
                }
            }
        }

        public static void SetPropertyValue<T>(this T target, object value, Expression memberLamda)
        {
            var memberSelectorExpression = memberLamda as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    if (property.CanWrite)
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(target, convertedValue, null);
                    }
                }
            }
        }

        public static string GetPropertyDisplayName<T, TKey>(Expression<Func<T, TKey>> selector)
        {
            var memberExpression = selector.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Selector must be of type MemberExpression");

            var attribute = memberExpression.Member.GetCustomAttribute<DisplayAttribute>();
            if (attribute == null)
                return memberExpression.Member.Name;
            return attribute.Name;
        }

        /// <summary>
        /// https://stackoverflow.com/a/12317018/633945
        /// </summary>
        /// <returns></returns>
        public static FieldInfo[] GetFieldInfosIncludingBaseClasses(Type type, BindingFlags bindingFlags)
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }
            else
            {   // Otherwise, collect all types up to the furthest base class
                var currentType = type;
                var fieldComparer = new FieldInfoComparer();
                var fieldInfoList = new HashSet<FieldInfo>(fieldInfos, fieldComparer);
                while (currentType != typeof(object))
                {
                    fieldInfos = currentType.GetFields(bindingFlags);
                    fieldInfoList.UnionWith(fieldInfos);
                    currentType = currentType.BaseType;
                }
                return fieldInfoList.ToArray();
            }
        }

        class FieldInfoComparer : IEqualityComparer<FieldInfo>
        {
            public bool Equals(FieldInfo x, FieldInfo y)
            {
                return x.DeclaringType == y.DeclaringType && x.Name == y.Name;
            }

            public int GetHashCode(FieldInfo obj)
            {
                return obj.Name.GetHashCode() ^ obj.DeclaringType.GetHashCode();
            }
        }
    }

    public class PropertyMatch
    {
        public PropertyInfo SourceProperty { get; set; }
        public PropertyInfo DestinationProperty { get; set; }
        public PropertyMapAttribute PropertyMap { get; set; }
        public PropertyCopyOptions CopyOptions { get; set; }
        public bool IsSourceArray { get; set; }
        public bool IsDestinationArray { get; set; }
        public int ArrayIndex { get; set; }

        public PropertyMatch(PropertyCopyOptions options)
        {
            CopyOptions = options;
        }

        public object GetSourceValue(object source)
        {
            if (!IsSourceArray)
                return SourceProperty.GetValue(source);
            var array = (Array)SourceProperty.GetValue(source);
            if (array == null)
            {
                var elementType = SourceProperty.PropertyType.GetElementType();
                if (elementType.IsValueType)
                    return Activator.CreateInstance(elementType);
                return null;
            }
            return array.GetValue(ArrayIndex);
        }

        public void SetDestinationValue(object destination, object value)
        {
            if (PropertyMap != null && PropertyMap.Converter != null)
            {
                PropertyMap.Converter.SourceProperty = SourceProperty;
                PropertyMap.Converter.DestinationProperty = DestinationProperty;
                value = PropertyMap.Converter.ConvertGeneric(value, PropertyMap.Converter.SourceProperty.PropertyType);
            }

            if (!IsDestinationArray || (CopyOptions?.UsePlainArrays ?? false))
            {
                if (DestinationProperty.CanWrite)
                {
                    DestinationProperty.SetValue(destination, value);
                }
                return;
            }
            var array = (Array)DestinationProperty.GetValue(destination);
            array.SetValue(value, ArrayIndex);
        }

        public void TransferValue(object source, object destination)
        {
            var value = GetSourceValue(source);
            SetDestinationValue(destination, value);
        }
    }

    public class PropertyMapAttribute : Attribute
    {
        public string Source { get; set; }
        public string[] ArraySource { get; set; }
        public PropertyConverter Converter { get; set; }
        public PropertySource DynamicSource { get; set; }

        public bool IsArraySource
        {
            get
            {
                return ArraySource != null;
            }
        }


        public PropertyMapAttribute(params string[] sources)
        {
            if (sources == null)
                return;
            if (sources.Length == 1)
            {
                Source = sources[0];
                return;
            }
            ArraySource = sources;
        }

        public PropertyMapAttribute(string source, Type converterType)
        {
            Source = source;
            if (converterType == null)
                throw new ArgumentException("Type is null");
            if (!typeof(PropertyConverter).IsAssignableFrom(converterType))
                throw new ArgumentException("Type is not a Subclass of Muffin.Common.Util.PropertyConverter<>");
            Converter = (PropertyConverter)Activator.CreateInstance(converterType);
        }

        public PropertyMapAttribute(Type propertySource)
        {
            if (propertySource == null)
                throw new ArgumentException("Type is null");
            if (!typeof(PropertySource).IsAssignableFrom(propertySource))
                throw new ArgumentException("Type is not a Subclass of Muffin.Common.Util.PropertySource<>");
            DynamicSource = (PropertySource)Activator.CreateInstance(propertySource);
        }

        public PropertyMapAttribute(Type propertySource, Type converterType)
        {
            if (propertySource == null)
                throw new ArgumentException("Type is null");
            if (!typeof(PropertySource).IsAssignableFrom(propertySource))
                throw new ArgumentException("Type is not a Subclass of Muffin.Common.Util.PropertySource<>");
            DynamicSource = (PropertySource)Activator.CreateInstance(propertySource);

            if (converterType == null)
                throw new ArgumentException("Type is null");
            if (!typeof(PropertyConverter).IsAssignableFrom(converterType))
                throw new ArgumentException("Type is not a Subclass of Muffin.Common.Util.PropertyConverter<>");
            Converter = (PropertyConverter)Activator.CreateInstance(converterType);
        }

        #region Helper

        public PropertyInfo GetPropertyInfo(object destination, int index)
        {
            var propertyName = ArraySource[index];
            var property = destination.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(propertyName));
            if (property == null)
                throw new NullReferenceException("Property not found: " + propertyName);
            return property;
        }

        public PropertyInfo GetPropertyInfo(object destination)
        {
            var property = destination.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(Source));

            if (property == null && DynamicSource != null)
                property = destination.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(DynamicSource.GetPropertyName(destination, x.Name))); // TODO auch füs das Array amchen

            if (property == null)
                throw new NullReferenceException("Property not found: " + Source);
            return property;
        }

        #endregion
    }

    public abstract class PropertyConverter
    {
        public PropertyInfo SourceProperty { get; set; }
        public PropertyInfo DestinationProperty { get; set; }

        public abstract object ConvertGeneric(object source, Type sourceType);
        public abstract bool MatchingTypes(Type type1, Type type2);
        public abstract object CastArray(object[] array);
        public abstract Type[] GetTypes();
    }

    public abstract class PropertyConverter<TSource, TDestination> : PropertyConverter
    {
        public override object ConvertGeneric(object source, Type sourceType)
        {
            var type = Nullable.GetUnderlyingType(sourceType);
            if (type != null && source == null)
                return default(TDestination);

            if (type == null)
                type = sourceType;

            if (type == typeof(TSource))
                return Convert((TSource)source);
            return Convert((TDestination)source);
        }

        public override bool MatchingTypes(Type type1, Type type2)
        {
            return (type1 == typeof(TSource) || type2 == typeof(TSource)) && (type1 == typeof(TDestination) || type2 == typeof(TDestination));
        }

        public override object CastArray(object[] array)
        {
            return array.Select(x => (TDestination)x).ToArray();
        }

        public override Type[] GetTypes()
        {
            return new Type[]
            {
                typeof(TSource),
                typeof(TDestination)
            };
        }

        public abstract TDestination Convert(TSource source);
        public abstract TSource Convert(TDestination source);
    }

    public abstract class PropertySource
    {
        public abstract string GetPropertyName(object destination, string destinationPropertyName);
    }

    public class PropertyCopyOptions
    {
        public bool UsePlainArrays { get; set; }
    }
}

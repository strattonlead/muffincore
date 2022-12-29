using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Muffin.Common.Util
{
    public static class JsonConvertExtensions
    {
        public static bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch { }
            result = default(T);
            return false;
        }

        /// <summary>
        /// Deserialize ohne Exception wenn es nicht klappt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string json)
        {
            if (TryDeserialize<T>(json, out var result))
            {
                return result;
            }
            return default;
        }
    }

    public class IdToStringConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //if (objectType.IsEnum)
            //{
            //    try
            //    {
            //        var enumValue = reader.ReadAsInt32();
            //        if (enumValue.HasValue)
            //        {
            //            return Convert.ChangeType(enumValue, objectType);
            //        }
            //    }
            //    catch { }
            //}

            JToken jt = JValue.ReadFrom(reader);
            var stringRepresentation = jt.Value<string>();
            if (string.IsNullOrWhiteSpace(stringRepresentation))
            {
                if (typeof(long?).Equals(objectType))
                {
                    return null;
                }
                else
                {
                    return 0L;
                }
            }
            try
            {
                return jt.Value<long>();
            }
            catch
            {
                if (typeof(long?).Equals(objectType))
                {
                    return null;
                }
                else
                {
                    return 0L;
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long).Equals(objectType) || typeof(long?).Equals(objectType) /*|| objectType.IsEnum*/;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                var longValue = value as long?;
                if (longValue.HasValue)
                {
                    serializer.Serialize(writer, value.ToString());
                }
                else
                {
                    serializer.Serialize(writer, null);
                }
            }
            catch { }
        }
    }

    public class LongToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return (long)reader.ReadAsDecimal().Value; // TODO!!!
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }

    public class IntegerToStringConterter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.ReadAsInt32().Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }

    public class DecimalToStringConterter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.ReadAsDecimal().Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }

    public class DatetimeToUnixTimeStringConterter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTimeExtensions.FromUnixTime((long)reader.ReadAsDecimal().Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, DateTimeExtensions.ToUnixTime((DateTime)value));
        }
    }

    /// <summary>
    /// https://stackoverflow.com/a/12202914
    /// 
    /// [JsonConverter(typeof(ConcreteTypeConverter<Something>))]
    /// public ISomething TheThing { get; set; }
    /// </summary>
    /// <typeparam name="TConcrete"></typeparam>
    public class ConcreteTypeConverter<TConcrete> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //assume we can convert to anything for now
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //explicitly specify the concrete type we want to create
            return serializer.Deserialize<TConcrete>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //use the default serialization - it works fine
            serializer.Serialize(writer, value);
        }
    }

    public class PreRegisteredTypeConverter : JsonConverter
    {
        public static void Register<TInterface, TImplementation>()
        {
            _types.Add(typeof(TInterface), typeof(TImplementation));
        }

        private static Dictionary<Type, Type> _types = new Dictionary<Type, Type>();

        public override bool CanConvert(Type objectType)
        {
            return _types.ContainsKey(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var implType = _types[objectType];
            return serializer.Deserialize(reader, implType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class InterfaceTypeConverter<TInterface, TImplementation> : JsonConverter
        where TImplementation : new()
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(TInterface).IsAssignableFrom(objectType);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            TImplementation target = Activator.CreateInstance<TImplementation>();

            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class LongJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType || typeof(long?) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            if (long.TryParse(value, out var result))
            {
                return result;
            }

            if (objectType == typeof(long?))
            {
                return null;
            }
            return 0;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}

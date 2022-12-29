using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Muffin.Rete
{
    public class NodeData
    {
        public int id { get; set; }

        public string name { get; set; }

        [JsonConverter(typeof(SafeCollectionConverter))]
        public Dictionary<string, InputData> inputs { get; set; }

        [JsonConverter(typeof(SafeCollectionConverter))]
        public Dictionary<string, OutputData> outputs { get; set; } // "outputs": [],

        [JsonProperty(PropertyName = "data")]
        public dynamic data { get; set; }

        [JsonProperty(PropertyName = "position")]
        public decimal[] Position { get; set; } = new decimal[2];

        [JsonIgnore]
        public dynamic OutputData { get; set; }
        [JsonIgnore]
        public dynamic InputData { get; set; }

        [JsonIgnore]
        public bool IsProcessed { get; set; }
    }

    public class SafeCollectionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<JToken>(reader).ToObjectCollectionSafe(objectType, serializer);
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public static class SafeJsonConvertExtensions
    {
        public static object ToObjectCollectionSafe(this JToken jToken, Type objectType)
        {
            return ToObjectCollectionSafe(jToken, objectType, JsonSerializer.CreateDefault());
        }

        public static object ToObjectCollectionSafe(this JToken jToken, Type objectType, JsonSerializer jsonSerializer)
        {
            if (jToken.Type == JTokenType.Array)
            {
                return Activator.CreateInstance(objectType);
            }

            return jToken.ToObject(objectType, jsonSerializer);
        }

        public static T ToObjectCollectionSafe<T>(this JToken jToken)
        {
            return (T)ToObjectCollectionSafe(jToken, typeof(T));
        }

        public static T ToObjectCollectionSafe<T>(this JToken jToken, JsonSerializer jsonSerializer)
        {
            return (T)ToObjectCollectionSafe(jToken, typeof(T), jsonSerializer);
        }
    }
}

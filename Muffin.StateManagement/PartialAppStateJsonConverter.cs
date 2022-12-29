using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Muffin.StateManagement
{
    public class PartialAppStateJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PartialAppState);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var partialAppState = (PartialAppState)value;
            var array = JArray.FromObject(partialAppState.Changes.Values.ToArray(), serializer);
            array.WriteTo(writer);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var appStateChanges = (AppStateChange[])serializer.Deserialize(reader, typeof(AppStateChange[]));
            var result = new PartialAppState();
            result.Changes = appStateChanges.ToDictionary(x => x.Path);
            return result;
        }
    }
}

using Newtonsoft.Json;

namespace Muffin.Rete
{
    public class Component
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Dictionary<string, object> Data { get; set; }

        [JsonProperty(PropertyName = "jsScript")]
        public string JsScript { get; set; }
    }
}
using Muffin.EntityFrameworkCore.Entity;
using Muffin.Rete;
using Newtonsoft.Json;

namespace Muffin.EntityFrameworkCore.Rete
{
    public class ComponentEntity : BaseEntity, IComponent
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Dictionary<string, object> Data { get; set; }

        [JsonProperty(PropertyName = "jsScript")]
        public string JsScript { get; set; }
    }
}
using Muffin.EntityFrameworkCore.Entity;
using Muffin.Rete;
using Newtonsoft.Json;

namespace Muffin.EntityFrameworkCore.Rete
{
    public class NodeDataEntity : BaseEntity, INodeData
    {

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "inputs"), JsonConverter(typeof(SafeCollectionConverter))]
        public Dictionary<string, InputData> Inputs { get; set; }

        [JsonProperty(PropertyName = "outputs"), JsonConverter(typeof(SafeCollectionConverter))]
        public Dictionary<string, OutputData> Outputs { get; set; } // "outputs": [],

        [JsonProperty(PropertyName = "data")]
        public Dictionary<string, object> Data { get; set; }

        [JsonProperty(PropertyName = "position")]
        public int[] Position { get; set; } = new int[2];

        [JsonIgnore]
        public dynamic OutputData { get; set; }
        [JsonIgnore]
        public dynamic InputData { get; set; }
    }
}

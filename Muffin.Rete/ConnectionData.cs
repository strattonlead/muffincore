using Newtonsoft.Json;

namespace Muffin.Rete
{
    public class ConnectionData
    {
        [JsonProperty(PropertyName = "node")]
        public int Node { get; set; }

        [JsonIgnore]
        public dynamic Data { get; set; }
    }

    public class InputData
    {
        [JsonProperty(PropertyName = "connections")]
        public List<InputConnectionData> Connections { get; set; }
    }

    public class OutputData
    {
        [JsonProperty(PropertyName = "connections")]
        public List<OutputConnectionData> Connections { get; set; }

        [JsonIgnore]
        public bool IsProcessed { get; set; }
    }

    public class InputConnectionData : ConnectionData
    {
        [JsonProperty(PropertyName = "output")]
        public string Output { get; set; }

        [JsonIgnore]
        public bool IsProcessed { get; set; }
    }

    public class OutputConnectionData : ConnectionData
    {
        [JsonProperty(PropertyName = "input")]
        public string Input { get; set; }

        [JsonIgnore]
        public bool IsProcessed { get; set; }
    }
}

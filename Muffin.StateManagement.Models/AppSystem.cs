using Newtonsoft.Json;

namespace Muffin.StateManagement.Models
{
    public class AppSystem
    {
        [JsonProperty(PropertyName = "dbConnectionAvailable", NullValueHandling = NullValueHandling.Ignore)]
        public bool DbConnectionAvailable { get; set; }

        [JsonProperty(PropertyName = "environment")]
        public string Environment { get; set; }

        [JsonProperty(PropertyName = "debug")]
        public bool Debug { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = "1.0.0.0";
    }
}

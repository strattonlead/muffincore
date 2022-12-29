using Newtonsoft.Json;

namespace Muffin.StateManagement.Models
{
    public class AppSignInOption
    {
        [JsonProperty(PropertyName = "active")]
        public bool IsActive { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}

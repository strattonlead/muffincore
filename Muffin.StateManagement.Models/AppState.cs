using Newtonsoft.Json;

namespace Muffin.StateManagement.Models
{
    public abstract class AppState
    {

        [JsonProperty(PropertyName = "userContext", NullValueHandling = NullValueHandling.Ignore)]
        public object UserContext { get; set; }

        [JsonProperty(PropertyName = "auth", NullValueHandling = NullValueHandling.Ignore)]
        public AppAuthenticationInfo AuthenticationInfo { get; set; }

        [JsonProperty(PropertyName = "system")]
        public AppSystem System { get; set; }
    }
}
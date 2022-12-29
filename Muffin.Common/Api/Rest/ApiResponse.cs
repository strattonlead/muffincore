using Muffin.Common.Api.WebSockets;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Muffin.Common.Api.Rest
{
    public class ApiResponse
    {
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "errorDetails")]
        public string ErrorDetails { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int? ErrorCode { get; set; }

        [JsonProperty(PropertyName = "js", NullValueHandling = NullValueHandling.Ignore)]
        public string Script { get; set; }

        [JsonProperty(PropertyName = "notifications", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Notification> Notifications { get; set; }

        [JsonProperty(PropertyName = "appState", NullValueHandling = NullValueHandling.Ignore)]
        public object AppStateChanges { get; set; }

        [JsonIgnore]
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public void Throw()
        {
            if (ErrorMessage != null)
            {
                throw new System.Exception(ErrorMessage + " " + ErrorDetails);
            }
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }
    }
}

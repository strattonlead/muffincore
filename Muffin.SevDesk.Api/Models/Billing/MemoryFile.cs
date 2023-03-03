using Newtonsoft.Json;

namespace Muffin.SevDesk.Api.Models.Billing
{
    public class MemoryFile
    {
        [JsonProperty(PropertyName = "filename")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "mimeType")]
        public string MimeType { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "base64encoded")]
        public string Base64encoded { get; set; }
    }
}

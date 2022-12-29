using Newtonsoft.Json;
using System.Collections.Generic;

namespace Muffin.WebSockets.Server.ModelBinding
{
    public class FormDataError
    {

        [JsonProperty(PropertyName = "error-messages")]
        public virtual List<object> Messages { get; set; } = new List<object>();
    }
}

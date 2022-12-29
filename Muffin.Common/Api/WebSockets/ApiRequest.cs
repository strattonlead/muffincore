using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.Common.Api.WebSockets
{
    public class ApiRequest
    {
        [JsonProperty(PropertyName = "requestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "controller", NullValueHandling = NullValueHandling.Ignore)]
        public string Controller { get; set; }

        [JsonProperty(PropertyName = "channelId", NullValueHandling = NullValueHandling.Ignore)]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "params")]
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic Result { get; set; }

        // jetzt alles in params
        //[JsonProperty(PropertyName = "errorMsg", NullValueHandling = NullValueHandling.Ignore)]
        //public string ErrorMessage { get; set; }

        //[JsonProperty(PropertyName = "errorCode", NullValueHandling = NullValueHandling.Ignore)]
        //public string ErrorCode { get; set; }

        //[JsonProperty(PropertyName = "notification", NullValueHandling = NullValueHandling.Ignore)]
        //public Notification Notification { get; set; }

        [JsonProperty(PropertyName = "url", NullValueHandling = NullValueHandling.Ignore)]
        public string RedirectionUrl { get; set; }

        //[JsonProperty(PropertyName = "js", NullValueHandling = NullValueHandling.Ignore)]
        //public string Script { get; set; }

        //[JsonProperty(PropertyName = "appState", NullValueHandling = NullValueHandling.Ignore)]
        //public object AppStateChanges { get; set; }

#warning ACHTUNG LEGACY SUPPORT DAS FLIEGT RAUS SOBALD DIE APP AKTUALISIERT WURDE
        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonIgnore]
        public bool HasError => Params?.ContainsKey("error") ?? false;

        [JsonIgnore]
        public string Error => HasError ? Params["error"]?.ToString() : null;

        public ApiRequest() { }
        public ApiRequest(string action)
        {
            Action = action;
        }
        public ApiRequest(string action, string controller)
            : this(action)
        {
            Controller = controller;
        }

        public ApiRequest(string action, string controller, string requestId)
            : this(action, controller)
        {
            RequestId = requestId;
        }

        public void SetParam<T>(string name, T value)
        {
            if (Params == null)
            {
                Params = new Dictionary<string, object>();
            }
            Params[name] = value;
        }

        public ApiRequest MakeResponse()
        {
            return new ApiRequest() { RequestId = RequestId };
        }

        //public ApiRequest MakeGeneric(object obj)
        //{
        //    var genericType = typeof(ApiRequest<>)
        //        .MakeGenericType(obj.GetType());

        //    var model = Activator.CreateInstance(genericType, new object[] { Action });
        //    var pi = obj.GetType()
        //        .GetProperties()
        //        .FirstOrDefault(x => string.Equals(x.Name, "Model"));
        //    pi.SetValue(model, obj);
        //    return (ApiRequest)model;
        //}

        public T GetResult<T>()
        {
            object obj = Result;
            if (Result == null && Params != null)
            {
                if (Params.TryGetValue("data", out obj)) ;
                else if (Params.TryGetValue("model", out obj)) ;
                else if (Params.Values.Count > 0)
                {
                    obj = Params.Values.FirstOrDefault();
                }
            }

            if (obj != null)
            {
                var json = JsonConvert.SerializeObject(obj);
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch { }
            }

            return default;
        }
    }

    public class Notification
    {
        [JsonProperty(PropertyName = "text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        /// <summary>
        /// https://vuetifyjs.com/en/styles/colors#
        /// </summary>
        [JsonProperty(PropertyName = "color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Muffin.Common.Util
{
    public static class HttpClientJsonHelper
    {
        public static T GetJson<T>(this WebClient client, string address)
        {
            var result = client.DownloadString(address);
            if (result == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(result);
        }

        public static T GetJson<T>(this HttpClient client, string address)
        {
            var result = client.GetStringAsync(address).Result;
            if (result == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(result);
        }

        public static T PostJson<T>(this HttpClient client, string address, object sendObject)
        {
            var jsonObject = JsonConvert.SerializeObject(sendObject);
            var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
            var responseMessage = client.PostAsync(address, content).Result;
            var result = responseMessage.Content.ReadAsStringAsync().Result;
            if (result == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(result);
        }

        public static T PostFormData<T>(this HttpClient client, string address, Dictionary<string, object> sendObj)
        {
            var result = PostFormData(client, address, sendObj);
            if (result == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(result);
        }

        public static string PostFormData(this HttpClient client, string address, Dictionary<string, object> sendObj)
        {
            var formParameters = sendObj.Select(x => new KeyValuePair<string, string>(x.Key, x.Value != null ? x.Value.ToString() : ""));
            var formContent = new FormUrlEncodedContent(formParameters);

            var responseMessage = client.PostAsync(address, formContent).Result;
            return responseMessage.Content.ReadAsStringAsync().Result;
        }


    }
}

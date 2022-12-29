using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Client
{
    public class HttpClientHelper
    {
        #region properties

        public readonly HttpClientHelperOptions Options;

        #endregion

        #region Constructor

        public HttpClientHelper(IServiceProvider serviceProvider)
        {
            Options = serviceProvider.GetRequiredService<HttpClientHelperOptions>();
        }

        #endregion

        #region Actions

        public string CreateUrl(string controller, string action)
        {
            return $"{Options.Url}/{controller}/{action}";
        }

        public async Task<TResult> PostJson<TResult>(HttpClient httpClient, string action, object model)
            where TResult : new()
        {
            return await PostJson<TResult>(httpClient, Options.DefaultController, action, model);
        }

        public async Task<TResult> PostJson<TResult>(HttpClient httpClient, string controller, string action, object model)
        where TResult : new()
        {
            var json = JsonConvert.SerializeObject(model);
            var formData = new StringContent(json, Encoding.UTF8, "application/json");
            var responseTask = httpClient.PostAsync(CreateUrl(controller, action), formData);

            var httpResponse = responseTask.GetAwaiter().GetResult();
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(httpResponse.ReasonPhrase);
            }

            json = await httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResult>(json);
        }

        #endregion
    }

    public class HttpClientHelperOptions
    {
        public string Url { get; set; }
        public string DefaultController { get; set; } = "Api";
    }

    public static class HttpClientHelperExtensions
    {
        public static void AddHttpClientHelper(this IServiceCollection services, Action<HttpClientHelperOptions> action)
        {
            if (action != null)
            {
                var options = new HttpClientHelperOptions();
                action(options);
                services.AddSingleton(options);
            }
            services.AddSingleton<HttpClientHelper>();
        }
    }

}

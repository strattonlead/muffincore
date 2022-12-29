using Microsoft.Extensions.DependencyInjection;
using Muffin.Deepl.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Muffin.Deepl
{
    public class DeeplService : IDeeplService
    {
        #region Properties

        private readonly DeeplServiceOptions Options;

        #endregion

        #region Constructor

        public DeeplService(IServiceProvider serviceProvider)
        {
            Options = serviceProvider.GetRequiredService<DeeplServiceOptions>();
        }

        #endregion

        #region IDeeplService

        public async Task<UsageResult> UsageAsync()
        {
            using (var httpClient = new HttpClient())
            {
                using (var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), $"{Options.ApiEndpoint}usage"))
                {
                    var contentList = new List<string>();
                    contentList.Add($"auth_key={Options.ApiKey}");
                    requestMessage.Content = new StringContent(string.Join("&", contentList));
                    requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(requestMessage);
                    if (!response.IsSuccessStatusCode)
                    {

                    }

                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UsageResult>(content);
                }
            }
        }

        public async Task<DeeplResult[]> TranslateAsync(DeeplRequest request)
        {
            using (var httpClient = new HttpClient())
            {
                using (var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), $"{Options.ApiEndpoint}translate"))
                {
                    var contentList = new List<string>();
                    contentList.Add($"auth_key={Options.ApiKey}");
                    if (request.Texts?.Any() ?? false)
                    {
                        foreach (var text in request.Texts)
                        {
                            contentList.Add($"text={text}");
                        }
                    }
                    else
                    {
                        contentList.Add($"text={request.Text}");
                    }

                    contentList.Add($"target_lang={request.TargetLang}");
                    if (!string.IsNullOrWhiteSpace(request.SourceLang))
                    {
                        contentList.Add($"source_lang={request.SourceLang}");
                    }
                    requestMessage.Content = new StringContent(string.Join("&", contentList));
                    requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(requestMessage);
                    if (!response.IsSuccessStatusCode)
                    {

                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var container = JsonConvert.DeserializeObject<DeeplResultContainer>(content);
                    foreach (var item in container.Translations)
                    {
                        item.TargetLang = request.TargetLang;
                    }

                    return container.Translations;
                }
            }
        }

        public async Task<DeeplResult[]> TranslateAsync(DeeplRequest[] requests)
        {
            var tasks = requests.Select(x => TranslateAsync(x)).ToArray();
            return (await Task.WhenAll(tasks)).SelectMany(x => x).ToArray();
        }

        public async Task<DeeplResult[]> TranslateAsync(MatrixRequest request)
        {
            var tasks = request.TargetLangs.Select(x => TranslateAsync(new DeeplRequest()
            {
                SourceLang = request.SourceLang,
                Texts = request.Texts,
                TargetLang = x
            })).ToArray();
            return (await Task.WhenAll(tasks)).SelectMany(x => x).ToArray();
        }

        #endregion
    }

    public class DeeplServiceOptions
    {
        public bool UsePro { get; set; }
        public string ApiKey { get; set; }

        public string ApiEndpoint => UsePro ? "https://api.deepl.com/v2/" : "https://api-free.deepl.com/v2/";
    }

    public static class DeeplServiceExtensions
    {
        public static void AddDeeplService(this IServiceCollection services, string apiKey, bool usePro)
        {
            services.AddSingleton(new DeeplServiceOptions()
            {
                UsePro = usePro,
                ApiKey = apiKey
            });
            services.AddSingleton<IDeeplService, DeeplService>();
        }
    }
}

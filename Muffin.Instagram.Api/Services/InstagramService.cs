using CreateIF.Instagram.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CreateIf.Instagram.Services
{
    public interface IInstagramService
    {
        Task<BusinessDiscoveryResult> BusinessDiscovery(IAccessToken accessToken, string username);
        Task<Dictionary<string, string>> Me(IAccessToken accessToken);
    }

    public class InstagramService : IInstagramService
    {
        private readonly InstagramOAuth2ServiceOptions _options;

        public InstagramService(IServiceProvider serviceProvider)
        {
            _options = serviceProvider.GetRequiredService<InstagramOAuth2ServiceOptions>();
        }

        public async Task<BusinessDiscoveryResult> BusinessDiscovery(IAccessToken accessToken, string username)
        {
            using (var httpClient = new HttpClient())
            {
                var url = InstagramConfiguration.GetBusinessDiscoveryUrl(_options.ApiVersion, accessToken.UserId, username, accessToken.AccessToken);
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = await httpClient.SendAsync(request);
                    var stringResult = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<BusinessDiscoveryResult>(stringResult);
                }
            }
        }

        public async Task<Dictionary<string, string>> Me(IAccessToken accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var url = InstagramConfiguration.GetMeUrl(accessToken.AccessToken, "account_type", "id", "media_count", "username");
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = await httpClient.SendAsync(request);
                    var stringResult = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResult);
                }
            }
        }



        //"https://graph.facebook.com/oauth/access_token?client_id=6066764640069422&client_secret=c7b147b6a82248bb37e50fa77fa47f46&grant_type=client_credentials"
    }

    public static class InstagramServiceExtensions
    {
        public static void AddInstagramService(this IServiceCollection services)
        {
            services.AddScoped<IInstagramService, InstagramService>();
        }
    }
}
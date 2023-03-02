using CreateIF.Instagram.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CreateIf.Instagram.Services
{
    public interface IInstagramOAuth2Service
    {
        string GetAuthUrl();
        Task<AccessTokenResponse> GetAccessToken(string code);
        Task<IAccessToken> GetLongLivedUserAccessToken(IAccessToken accessToken);
        Task<IAccessToken> RefreshAccessToken(IAccessToken accessToken);
    }

    public class InstagramOAuth2Service : IInstagramOAuth2Service
    {
        private readonly InstagramOAuth2ServiceOptions Options;

        public InstagramOAuth2Service(IServiceProvider serviceProvider)
        {
            Options = serviceProvider.GetRequiredService<InstagramOAuth2ServiceOptions>();
        }

        public string GetAuthUrl()
        {
            var scopes = string.Join(",", Options.Scopes);
            return InstagramConfiguration.GetAuthUrl(Options.AppId, Options.RedirectUri, scopes);
        }

        public async Task<AccessTokenResponse> GetAccessToken(string code)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), InstagramConfiguration.ACCESS_TOKEN_URL))
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(Options.AppId), "client_id");
                    content.Add(new StringContent(Options.AppSecet), "client_secret");
                    content.Add(new StringContent("authorization_code"), "grant_type");
                    content.Add(new StringContent(Options.RedirectUri), "redirect_uri");
                    content.Add(new StringContent(code), "code");
                    request.Content = content;

                    var response = await httpClient.SendAsync(request);
                    var stringResult = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AccessTokenResponse>(stringResult);
                }
            }
        }

        public async Task<IAccessToken> GetLongLivedUserAccessToken(IAccessToken accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var url = InstagramConfiguration.GetLongLivedUserAccessTokenUrl(Options.AppSecet, accessToken.AccessToken);
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = await httpClient.SendAsync(request);
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var accessTokenResponse = JsonConvert.DeserializeObject<UserAccessTokenResponse>(stringResult);
                    accessTokenResponse.UserId = accessToken.UserId;
                    return accessTokenResponse;
                }
            }
        }

        public async Task<IAccessToken> RefreshAccessToken(IAccessToken accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var url = InstagramConfiguration.GetRefreshAccessTokenUrl(accessToken.AccessToken);
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = await httpClient.SendAsync(request);
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var accessTokenResponse = JsonConvert.DeserializeObject<UserAccessTokenResponse>(stringResult);
                    accessTokenResponse.UserId = accessToken.UserId;
                    return accessTokenResponse;
                }
            }
        }
    }

    public class AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("error_type")]
        public string ErrorType { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("code")]
        public int? Code { get; set; }

        public bool Success => !Code.HasValue || !string.IsNullOrWhiteSpace(ErrorMessage);
    }

    public class BusinessDiscoveryResult
    {
        [JsonProperty("business_discovery")]
        public BusinessDiscovery BusinessDiscovery { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class BusinessDiscovery
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("followers_count")]
        public long FollowersCount { get; set; }
    }

    public interface IAccessToken
    {
        [JsonProperty("access_token")]
        string AccessToken { get; set; }

        [JsonProperty("token_type")]
        string TokenType { get; set; }

        [JsonProperty("expires_in")]
        int ExpiresIn { get; set; }

        [JsonIgnore]
        TimeSpan Expires { get; }

        [JsonIgnore]
        bool IsValid { get; }

        /// <summary>
        /// The ig user id
        /// </summary>
        [JsonIgnore]
        string UserId { get; set; }
    }

    public class UserAccessTokenResponse : IAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonIgnore]
        public TimeSpan Expires => TimeSpan.FromSeconds(ExpiresIn);

        [JsonIgnore]
        public bool IsValid => ExpiresIn > 0;

        [JsonIgnore]
        public string UserId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CreateIF.Instagram.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CreateIf.Instagram.Services
{
    public interface IInstagramOAuth2Service
    {
        string GetAuthUrl();
        Task<AccessTokenResponse> GetAccessToken(string code);
        Task<IAccessToken> GetLongLivedUserAccessToken(string accessToken);
        Task<IAccessToken> RefreshAccessToken(string longLivedAccessToken);
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

        public async Task<IAccessToken> GetLongLivedUserAccessToken(string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var url = InstagramConfiguration.GetLongLivedUserAccessTokenUrl(Options.AppSecet, accessToken);
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = await httpClient.SendAsync(request);
                    var stringResult = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserAccessTokenResponse>(stringResult);
                }
            }
        }

        public async Task<IAccessToken> RefreshAccessToken(string longLivedAccessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var url = InstagramConfiguration.GetRefreshAccessTokenUrl(longLivedAccessToken);
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = await httpClient.SendAsync(request);
                    var stringResult = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserAccessTokenResponse>(stringResult);
                }
            }
        }
    }

    public class AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("error_type")]
        public string ErrorType { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("code")]
        public int? Code { get; set; }

        public bool Success => !Code.HasValue || !string.IsNullOrWhiteSpace(ErrorMessage);
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
        public bool IsValid { get; }
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
    }
}
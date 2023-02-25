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
    public class InstagramOAuth2ServiceOptionsBuilder
    {
        internal InstagramOAuth2ServiceOptions _options = new InstagramOAuth2ServiceOptions();

        public InstagramOAuth2ServiceOptionsBuilder UseAppId(string appId)
        {
            _options.AppId = appId;
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseAppSecret(string appSecret)
        {
            _options.AppSecet = appSecret;
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseUserProfileScope()
        {
            return UseScopes(InstagramConfiguration.Scopes.USER_PROFILE);
        }

        public InstagramOAuth2ServiceOptionsBuilder UseUserMediaScope()
        {
            return UseScopes(InstagramConfiguration.Scopes.USER_MEDIA);
        }

        public InstagramOAuth2ServiceOptionsBuilder UseScopes(params string[] scopes)
        {
            _options.Scopes.AddRange(scopes);
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseRedirectUri(string redirectUri)
        {
            _options.RedirectUri = redirectUri;
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseSuccessHandler(string redirectUri)
        {
            _options.RedirectUri = redirectUri;
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseErrorHandler(string redirectUri)
        {
            _options.RedirectUri = redirectUri;
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseSuccessRedirectionHandler(Func<string, HttpContext, IActionResult> handler)
        {
            _options.Events.OnAuthSuccessRedirection += (s, h) => { return handler(s, h); };
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseSuccessHandler(Action<string, HttpContext> handler)
        {
            _options.Events.OnAuthSuccess += (s, h) => { handler(s, h); };
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseErrorRedirectionHandler(Func<Exception, HttpContext, IActionResult> handler)
        {
            _options.Events.OnAuthErrorRedirection += (s, h) => { return handler(s, h); };
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseErrorHandler(Action<Exception, HttpContext> handler)
        {
            _options.Events.OnAuthError += (s, h) => { handler(s, h); };
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseRefreshService<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TService : class, IInstagramAccessTokenService
            where TImplementation : class, TService
        {
            _options.UseRefreshService = true;
            _options.RefreshServiceType = typeof(TService);
            _options.RefreshServiceImplementationType = typeof(TImplementation);
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseRefreshServiceInterval(TimeSpan refreshInterval)
        {
            _options.RefreshServiceInterval = refreshInterval;
            return this;
        }

        public InstagramOAuth2ServiceOptionsBuilder UseMinAccessTokenLifeSpan(TimeSpan timeSpan)
        {
            _options.MinAccessTokenLifeSpan = timeSpan;
            return this;
        }

        public InstagramOAuth2ServiceOptions Build()
        {
            return _options;
        }
    }
}
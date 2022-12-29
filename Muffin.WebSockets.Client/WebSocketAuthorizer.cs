using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Client
{
    public abstract class WebSocketAuthorizer
    {
        #region Properties

        protected CookieContainer CookieContainer;
        protected readonly WebSocketAuthorizerOptions Options;
        protected readonly HttpClientProvider WebSocketHttpClientProvider;
        protected readonly WebSocketAuthorizerEvents AuthorizerEvents;
        public HttpClient HttpClient { get; private set; }

        #endregion

        #region Constructor

        public WebSocketAuthorizer(IServiceProvider serviceProvider)
        {
            var authorizerType = GetType();
            var optionsType = typeof(WebSocketAuthorizerOptions<>).MakeGenericType(authorizerType);
            Options = (WebSocketAuthorizerOptions)serviceProvider.GetRequiredService(optionsType);

            WebSocketHttpClientProvider = serviceProvider.GetRequiredService<HttpClientProvider>();

            var authorizerEventsType = typeof(WebSocketAuthorizerEvents<>).MakeGenericType(authorizerType);
            AuthorizerEvents = (WebSocketAuthorizerEvents)serviceProvider.GetRequiredService(authorizerEventsType);

            CookieContainer = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = CookieContainer;

            if (Options.UseProxy)
            {
                handler.UseProxy = true;
                handler.Proxy = new WebProxy(Options.ProxyHost, Options.ProxyPort);
                handler.Credentials = new NetworkCredential(Options.ProxyUsername, Options.ProxyPassword);
            }

            HttpClient = WebSocketHttpClientProvider.GetClient(handler);
        }

        #endregion

        #region Authorizer

        public virtual async Task<bool> Authorize()
        {
            var success = await AuthorizeCore(HttpClient);
            AuthorizerEvents?.InvokeOnAuthorize(this, success);
            return success;
        }

        protected abstract Task<bool> AuthorizeCore(HttpClient client);

        public virtual IEnumerable<Cookie> GetCookies()
        {
            var cookieList = CookieContainer?.GetCookies(new Uri(Options.Url));
            foreach (Cookie item in cookieList)
            {
                yield return item;
            }
        }

        #endregion
    }

    public class WebSocketAuthorizerOptions
    {
        public string Url { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
    }

    public class WebSocketAuthorizerOptions<T> : WebSocketAuthorizerOptions
        where T : WebSocketAuthorizer
    { }

    public class WebSocketAuthorizerOptionsBuilder<T>
        where T : WebSocketAuthorizer
    {
        private WebSocketAuthorizerOptions<T> Options = new WebSocketAuthorizerOptions<T>();

        public WebSocketAuthorizerOptionsBuilder<T> UseProxy(string host, int port, string username, string password)
        {
            Options.UseProxy = true;
            Options.ProxyHost = host;
            Options.ProxyPort = port;
            Options.ProxyUsername = username;
            Options.ProxyPassword = password;
            return this;
        }

        public WebSocketAuthorizerOptionsBuilder<T> UseUrl(string url)
        {
            Options.Url = url;
            return this;
        }

        public WebSocketAuthorizerOptions<T> Build()
        {
            return Options;
        }
    }

    public class WebSocketAuthorizerEvents
    {
        public event EventHandler<bool> OnAuthorize;

        internal void InvokeOnAuthorize(object sender, bool success)
        {
            OnAuthorize?.Invoke(sender, success);
        }
    }

    public class WebSocketAuthorizerEvents<T> : WebSocketAuthorizerEvents
        where T : WebSocketAuthorizer
    { }

    public static class WebSocketAuthorizerExtenstions
    {
        public static void AddWebSocketAuthorizer<T>(this IServiceCollection services, Action<WebSocketAuthorizerOptionsBuilder<T>> action)
        where T : WebSocketAuthorizer
        {
            if (action != null)
            {
                var optionsBuilder = new WebSocketAuthorizerOptionsBuilder<T>();
                action(optionsBuilder);
                var options = optionsBuilder.Build();
                services.AddSingleton(options);
            }
            services.AddSingleton<WebSocketAuthorizerEvents<T>>();
            services.AddSingleton<HttpClientProvider>();
            services.AddSingleton<T>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WebSocketSharp.Net;

namespace Muffin.WebSockets.Client
{
    public class WebSocketCookieProvider<T>
        where T : WebSocketAuthorizer
    {
        #region Properties

        protected readonly T WebSocketAuthorizer;

        #endregion

        #region Constructor

        public WebSocketCookieProvider(IServiceProvider serviceProvider)
        {
            WebSocketAuthorizer = serviceProvider.GetRequiredService<T>();
        }

        #endregion

        #region Provider

        public Cookie[] GetCookies()
        {
            return WebSocketAuthorizer
                .GetCookies()
                .Select(x => new Cookie(x.Name, x.Value, x.Path, x.Domain))
                .ToArray();
        }

        #endregion
    }

    public static class WebSocketCookieProviderExtensions
    {
        public static void AddWebSocketCookieProvider<T>(this IServiceCollection services)
        where T : WebSocketAuthorizer
        {
            services.AddSingleton<WebSocketCookieProvider<T>>();
        }
    }
}

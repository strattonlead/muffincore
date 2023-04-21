using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Muffin.WebSockets.Server.Handler;
using System;
using System.Net.WebSockets;

namespace Muffin.WebSockets.Server.Services
{
    public interface IWebSocketContextAccessor
    {
        public Lazy<WebSocketContext> WebSocketContext { get; }
    }

    public class WebSocketContext
    {
        public WebSocket WebSocket { get; set; }
        public SocketInfo WebSocketInfo { get; set; }
    }

    public class WebSocketContextAccessor : IWebSocketContextAccessor
    {
        #region Properties

        protected readonly IHttpContextAccessor HttpContextAccessor;

        #endregion

        #region Constructor

        public WebSocketContextAccessor(IServiceProvider serviceProvider)
        {
            HttpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            WebSocketContext = new Lazy<WebSocketContext>(() =>
            {
                if (HttpContextAccessor != null
                    && HttpContextAccessor.HttpContext != null
                    && HttpContextAccessor.HttpContext.WebSockets != null
                    && HttpContextAccessor.HttpContext.WebSockets.IsWebSocketRequest)
                {
                    var webSocketFeature = HttpContextAccessor.HttpContext.Features.Get<WebSocketFeature>();
                    if (webSocketFeature != null)
                    {
                        return new WebSocketContext()
                        {
                            WebSocket = webSocketFeature.WebSocket,
                            WebSocketInfo = webSocketFeature.WebSocketInfo
                        };
                    }
                }
                return null;
            });
        }

        #endregion

        #region IWebSocketContextAccessor

        public Lazy<WebSocketContext> WebSocketContext { get; protected set; }


        #endregion
    }

    public static class WebSocketContextAccessorExtensions
    {
        public static void AddWebSocketContextAccessor(this IServiceCollection services)
        {
            services.AddScoped<IWebSocketContextAccessor, WebSocketContextAccessor>();
        }
    }
}

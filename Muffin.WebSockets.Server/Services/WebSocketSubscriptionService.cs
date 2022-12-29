using Microsoft.Extensions.DependencyInjection;
using Muffin.WebSockets.Server.Handler;
using System;

namespace Muffin.WebSockets.Server.Services
{
    public class WebSocketSubscriptionService
    {
        #region Properties

        private readonly WebSocketConnectionManager WebSocketConnectionManager;
        private readonly IWebSocketContextAccessor WebSocketContextAccessor;

        #endregion

        #region Properties

        public WebSocketSubscriptionService(IServiceProvider serviceProvider)
        {
            WebSocketConnectionManager = serviceProvider.GetService<WebSocketConnectionManager>();
            WebSocketContextAccessor = serviceProvider.GetService<IWebSocketContextAccessor>();
        }

        #endregion

        #region Actions

        public void Subscribe(string channelId)
        {
            Subscribe(WebSocketContextAccessor?.WebSocketContext.Value?.WebSocketInfo?.SocketId, channelId);
        }

        public void Unsubscribe(string channelId)
        {
            Unsubscribe(WebSocketContextAccessor?.WebSocketContext.Value?.WebSocketInfo?.SocketId, channelId);
        }

        public void UnsubscribeAll()
        {
            WebSocketContextAccessor?.WebSocketContext.Value?.WebSocketInfo?.UnsubscribeAll();
        }

        public void Subscribe(string socketId, string channelId)
        {
            var socketInfo = WebSocketConnectionManager?.Connections.GetSocketInfo(socketId);
            socketInfo?.Subscribe(channelId);
        }

        public void Unsubscribe(string socketId, string channelId)
        {
            var socketInfo = WebSocketConnectionManager?.Connections.GetSocketInfo(socketId);
            socketInfo?.Unsubscribe(channelId);
        }

        public void UnsubscribeAll(string socketId)
        {
            var socketInfo = WebSocketConnectionManager?.Connections.GetSocketInfo(socketId);
            socketInfo?.UnsubscribeAll();
        }

        #endregion
    }

    public static class WebSocketSubscriptionServiceExtensions
    {
        public static void AddWebSocketSubscriptionService(this IServiceCollection services)
        {
            services.AddScoped<WebSocketSubscriptionService>();
        }
    }
}

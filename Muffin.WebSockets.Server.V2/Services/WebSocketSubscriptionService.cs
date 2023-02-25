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

        public void Subscribe(string topic)
        {
            Subscribe(WebSocketContextAccessor?.WebSocketContext.Value?.WebSocketConnection?.Id, topic);
        }

        public void Unsubscribe(string topic)
        {
            Unsubscribe(WebSocketContextAccessor?.WebSocketContext.Value?.WebSocketConnection?.Id, topic);
        }

        public void UnsubscribeAll()
        {
            WebSocketContextAccessor?.WebSocketContext.Value?.WebSocketConnection?.UnsubscribeAll();
        }

        public void Subscribe(string connectionId, string topic)
        {
            var webSocketConnection = WebSocketConnectionManager?.Connections.GetWebSocketConnection(connectionId);
            webSocketConnection?.Subscribe(topic);
        }

        public void Unsubscribe(string connectionId, string topic)
        {
            var webSocketConnection = WebSocketConnectionManager?.Connections.GetWebSocketConnection(connectionId);
            webSocketConnection?.Unsubscribe(topic);
        }

        public void UnsubscribeAll(string connectionId)
        {
            var webSocketConnection = WebSocketConnectionManager?.Connections.GetWebSocketConnection(connectionId);
            webSocketConnection?.UnsubscribeAll();
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

using Microsoft.Extensions.DependencyInjection;
using Muffin.WebSockets.Server.Handler;

namespace Muffin.WebSockets.Server.Queue
{
    public static class MessageHandlerQueueExtensions
    {
        public static void AddMessageHandlerQueue<TWebSocketHandler, TClientInterface>(this IServiceCollection services)
            where TWebSocketHandler : WebSocketConnectionHandler
        {
            services.AddSingleton<MessageHandlerQueue<TWebSocketHandler, TClientInterface>>();
            services.AddSingleton<MessageHandlerQueueBackgroundServiceController<TWebSocketHandler, TClientInterface>>();
            //services.AddSingleton<MessageHandlerQueueEvents<TClientInterface>>();
            services.AddHostedService<MessageHandlerQueueBackgroundService<TWebSocketHandler, TClientInterface>>();
        }
    }
}

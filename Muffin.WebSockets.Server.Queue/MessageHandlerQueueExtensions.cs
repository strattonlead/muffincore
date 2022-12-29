using Microsoft.Extensions.DependencyInjection;
using Muffin.WebSockets.Server.Handler;

namespace Muffin.WebSockets.Server.Queue
{
    public static class MessageHandlerQueueExtensions
    {
        public static void AddMessageHandlerQueue<TWebSocketHandler, TClientInterface>(this IServiceCollection services)
            where TWebSocketHandler : WebSocketHandler
        {
            services.AddSingleton<MessageHandlerQueue<TWebSocketHandler, TClientInterface>>();
            services.AddSingleton<MessageHandlerQueueEvents<TClientInterface>>();
        }
    }
}

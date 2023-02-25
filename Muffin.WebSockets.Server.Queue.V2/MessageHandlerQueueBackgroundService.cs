using Microsoft.Extensions.DependencyInjection;
using Muffin.BackgroundServices;
using Muffin.WebSockets.Server.Handler;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Queue
{
    public class MessageHandlerQueueBackgroundService<TWebSocketHandler, TClientInterface> : EventBackgroundService
            where TWebSocketHandler : WebSocketConnectionHandler
    {
        private readonly MessageHandlerQueue<TWebSocketHandler, TClientInterface> Queue;
        private readonly MessageHandlerQueueBackgroundServiceController<TWebSocketHandler, TClientInterface> Controller;

        public MessageHandlerQueueBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Queue = serviceProvider.GetRequiredService<MessageHandlerQueue<TWebSocketHandler, TClientInterface>>();
            Controller = serviceProvider.GetRequiredService<MessageHandlerQueueBackgroundServiceController<TWebSocketHandler, TClientInterface>>();
            Controller.OnForceRun += OnForceRun;
            RunOnStartup = true;
        }

        protected override async Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            while (Queue.QueueItems.Any())
            {
                if (Queue.QueueItems.TryDequeue(out var webSocketSend))
                {
                    await webSocketSend.Send();
                }
            }
        }
    }
}

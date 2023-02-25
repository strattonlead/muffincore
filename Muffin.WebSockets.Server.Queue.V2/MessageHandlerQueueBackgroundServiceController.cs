using Muffin.WebSockets.Server.Handler;

namespace Muffin.WebSockets.Server.Queue
{
    public class MessageHandlerQueueBackgroundServiceController<TWebSocketHandler, TClientInterface>
            where TWebSocketHandler : WebSocketConnectionHandler
    {
        public MessageHandlerQueueBackgroundServiceControllerEvent OnForceRun;
        public void ForceRun()
        {
            OnForceRun?.Invoke();
        }
    }

    public delegate void MessageHandlerQueueBackgroundServiceControllerEvent();
}

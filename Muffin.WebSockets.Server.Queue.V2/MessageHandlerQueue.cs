using Microsoft.Extensions.DependencyInjection;
using Muffin.Common.Api.WebSockets;
using Muffin.WebSockets.Server.Handler;
using Muffin.WebSockets.Server.Invocation;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Muffin.WebSockets.Server.Queue
{
    public interface IMessageHandlerQueue
    {
        WebSocketHandlerEvents WebSocketHandlerEvents { get; }
        void SubmitEnqueue(WebSocketSend webSocketSend);
    }

    public class MessageHandlerQueue<TWebSocketHandler, TClientInterface> : IMessageHandlerQueue
        where TWebSocketHandler : WebSocketConnectionHandler
    {
        #region Properties

        private readonly TWebSocketHandler MessageHandler;
        internal readonly ConcurrentQueue<WebSocketSend> QueueItems;
        private readonly MessageHandlerQueueBackgroundServiceController<TWebSocketHandler, TClientInterface> Controller;
        public WebSocketHandlerEvents WebSocketHandlerEvents { get; private set; }

        #endregion

        #region Constructor

        public MessageHandlerQueue(IServiceProvider serviceProvider)
        {
            MessageHandler = serviceProvider.GetRequiredService<TWebSocketHandler>();
            WebSocketHandlerEvents = serviceProvider.GetService<WebSocketHandlerEvents>();
            QueueItems = new ConcurrentQueue<WebSocketSend>();
            Controller = serviceProvider.GetRequiredService<MessageHandlerQueueBackgroundServiceController<TWebSocketHandler, TClientInterface>>();
        }

        #endregion

        #region Actions

        public QueueSendBuilder<TClientInterface> CreateEnqueue()
        {
            return new QueueSendBuilder<TClientInterface>(MessageHandler, this);
        }

        public void SubmitEnqueue(WebSocketSend webSocketSend)
        {
            QueueItems.Enqueue(webSocketSend);
            Controller.ForceRun();
        }

        #endregion
    }

    public class WebSocketPromise<T>
    {
        private readonly WebSocketHandlerEvents WebSocketHandlerEvents;
        public readonly string RequestId;
        private ApiRequest ApiRequest;
        //private Dictionary<string, object> ResponseParameters;
        private ManualResetEvent ResetEvent = new ManualResetEvent(false);
        public bool IsCompleted { get; private set; }
        public bool HasError => IsCompleted && Error != null;

        private T Result;
        public string Error { get; private set; }

        public WebSocketPromise(string requestId, WebSocketHandlerEvents webSocketHandlerEvents)
        {
            RequestId = requestId;
            WebSocketHandlerEvents = webSocketHandlerEvents;
            WebSocketHandlerEvents.OnApiReceive += WebSocketHandlerEvents_OnApiReceive;
        }

        ~WebSocketPromise()
        {
            WebSocketHandlerEvents.OnApiReceive -= WebSocketHandlerEvents_OnApiReceive;
        }

        private void WebSocketHandlerEvents_OnApiReceive(WebSocketConnectionHandler sender, ReceiveApiReventArgs args)
        {
            if (args?.ApiRequest?.RequestId != RequestId)
            {
                return;
            }

            //ResponseParameters = args.ApiRequest.Params;
            ApiRequest = args.ApiRequest;
            IsCompleted = true;
            ResetEvent.Set();
            GetResult();
        }

        public T GetResult(TimeSpan timeout)
        {
            return GetResult(timeout, CancellationToken.None);
        }

        public T GetResult()
        {
            return GetResult(null, CancellationToken.None);
        }

        public T GetResult(TimeSpan? timeout, CancellationToken cancellationToken = default)
        {
            if (Result == null)
            {
                Wait(timeout, cancellationToken);

                if (ApiRequest == null)
                {
                    Result = default;
                }
                else
                {
                    Result = ApiRequest.GetResult<T>();
                    Error = ApiRequest.Error;
                }
            }
            return Result;
        }

        public WebSocketPromise<T> Wait(TimeSpan? timeout, CancellationToken cancellationToken = default)
        {
            if (timeout.HasValue)
            {
                WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, ResetEvent }, timeout.Value);
            }
            else
            {
                WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, ResetEvent });
            }
            return this;
        }
    }
}

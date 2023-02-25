using Muffin.Common.Api.WebSockets;
using Muffin.WebSockets.Server.Extensions;
using Muffin.WebSockets.Server.Handler;
using Muffin.WebSockets.Server.Invocation;
using System;
using System.Linq.Expressions;

namespace Muffin.WebSockets.Server.Queue
{
    public class QueueSendBuilder<TClientInterface> : WebSocketSendBuilder
    {
        private readonly IMessageHandlerQueue Queue;

        public QueueSendBuilder(WebSocketConnectionHandler webSocketConnectionHandler, IMessageHandlerQueue queue)
            : base(webSocketConnectionHandler)
        {
            Queue = queue;
        }

        public WebSocketSendBuilder WithExpression(Expression<Action<TClientInterface>> expression)
        {
            ApiRequest apiRequest = ApiRequestExtensions.DynamicMethodCall(expression);
            return WithApiRequest(apiRequest);
        }

        public QueueSendBuilder<TClientInterface> WithPromise<T>(Expression<Func<TClientInterface, T>> func, out WebSocketPromise<T> promise)
        {
            var requestId = Guid.NewGuid().ToString();
            promise = new WebSocketPromise<T>(requestId, Queue.WebSocketHandlerEvents);

            var action = Expression.Lambda<Action<TClientInterface>>(func.Body, func.Parameters);
            var apiRequest = ApiRequestExtensions.DynamicMethodCall(action);
            apiRequest.RequestId = requestId;
            WithApiRequest(apiRequest);

            Queue.SubmitEnqueue(Build());
            return this;
        }

        public void Enqueue()
        {
            Queue.SubmitEnqueue(Build());
        }
    }
}

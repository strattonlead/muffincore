using Muffin.Common.Api.WebSockets;
using Muffin.WebSockets.Server.Extensions;
using Muffin.WebSockets.Server.Handler;
using Muffin.WebSockets.Server.Models;
using Muffin.WebSockets.Server.V2.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Invocation
{
    public class WebSocketSendBuilder
    {
        private WebSocketSend WebSocketSend { get; set; }

        public WebSocketSendBuilder(WebSocketConnectionHandler webSocketConnectionHandler)
        {
            WebSocketSend = new WebSocketSend(webSocketConnectionHandler);
        }

        public WebSocketSendBuilder WithWebSocketConnection(WebSocketConnection webSocketConnection)
        {
            if (webSocketConnection != null)
            {
                WebSocketSend.WebSocketConnections.Add(webSocketConnection);
            }
            return this;
        }

        public WebSocketSendBuilder WithTopic(string topic)
        {
            if (!string.IsNullOrEmpty(topic))
            {
                WebSocketSend.Topics.Add(topic);
            }
            return this;
        }

        public WebSocketSendBuilder WithTopic(IHasTopic topic)
        {
            return WithTopic(topic.Topic);
        }

        public WebSocketSendBuilder WithPrincipal(IPrincipal principal)
        {
            return WithIdentity(principal?.Identity?.Name);
        }

        public virtual WebSocketSendBuilder WithExpression<T>(Expression<Action<T>> expression)
        {
            var apiRequest = ApiRequestExtensions.DynamicMethodCall(expression);
            return WithApiRequest(apiRequest);
        }

        public WebSocketSendBuilder WithApiRequest(ApiRequest apiRequest)
        {
            return WithObject(apiRequest);
        }

        public WebSocketSendBuilder WithObject<T>(T obj)
        {
            WebSocketSend.Message = ApiRequestExtensions.Serialize(obj);
            return this;
        }

        public WebSocketSendBuilder WithIdentity(string identity)
        {
            if (!string.IsNullOrWhiteSpace(identity))
            {
                WebSocketSend.Identities.Add(identity);
            }
            return this;
        }

        public WebSocketSendBuilder WithMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                WebSocketSend.Message = message;
            }
            return this;
        }

        public WebSocketSendBuilder WithIgnoreSocketConnectionId(string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                WebSocketSend.IgnoreWebSocketConnectionIds.Add(connectionId);
            }
            return this;
        }

        public WebSocketSendBuilder WithIgnoreSocketConnectionIds(IEnumerable<string> connectionIds)
        {
            if (connectionIds != null)
            {
                WebSocketSend.IgnoreWebSocketConnectionIds.AddRange(connectionIds.Select(x => WebSocketConnectionId.Parse(x)));
            }
            return this;
        }

        public WebSocketSend Build()
        {
            return WebSocketSend;
        }

        public virtual async Task<WebSocketSendResult[]> Send()
        {
            return await Build().Send();
        }
    }
}

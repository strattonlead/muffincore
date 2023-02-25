using Muffin.WebSockets.Server.Handler;
using Muffin.WebSockets.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Invocation
{
    public class WebSocketSend
    {
        public List<string> Topics { get; private set; } = new List<string>();
        public List<string> Identities { get; private set; } = new List<string>();
        public List<WebSocketConnectionId> WebSocketConnectionIds { get; private set; } = new List<WebSocketConnectionId>();
        public List<WebSocketConnectionId> IgnoreWebSocketConnectionIds { get; private set; } = new List<WebSocketConnectionId>();
        public List<WebSocketConnection> WebSocketConnections { get; private set; } = new List<WebSocketConnection>();
        public string Message { get; set; }

        private readonly WebSocketConnectionHandler WebSocketConnectionHandler;

        public WebSocketSend(WebSocketConnectionHandler webSocketConnectionHandler)
        {
            WebSocketConnectionHandler = webSocketConnectionHandler;
        }

        public async Task<WebSocketSendResult[]> Send()
        {
            return await WebSocketConnectionHandler.Invoke(this);
        }
    }
}

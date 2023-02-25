using System.Net.WebSockets;

namespace Muffin.WebSockets.Server.Handler
{
    public class WebSocketFeature
    {
        #region Properties

        public WebSocket WebSocket => WebSocketConnection?.WebSocket;
        public readonly WebSocketConnection WebSocketConnection;

        #endregion

        #region Constructor

        public WebSocketFeature(WebSocketConnection webSocketConnection)
        {
            WebSocketConnection = webSocketConnection;
        }

        #endregion
    }
}

using System.Net.WebSockets;

namespace Muffin.WebSockets.Server.Handler
{
    public class WebSocketFeature
    {
        #region Properties

        public WebSocket WebSocket => WebSocketInfo?.WebSocket;
        public SocketInfo WebSocketInfo { get; set; }

        #endregion

        #region Constructor

        public WebSocketFeature(SocketInfo socketInfo)
        {
            WebSocketInfo = socketInfo;
        }

        #endregion
    }
}

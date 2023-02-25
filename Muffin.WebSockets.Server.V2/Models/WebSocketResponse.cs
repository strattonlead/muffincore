using Muffin.Common.Api.WebSockets;
using Muffin.WebSockets.Server.Handler;
using Muffin.WebSockets.Server.Invocation;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Models
{
    public class WebSocketResponse
    {
        protected WebSocketConnectionHandler Handler { get; set; }
        protected WebSocket Socket => WebSocketConnection?.WebSocket;
        protected WebSocketConnection WebSocketConnection { get; set; }
        internal WebSocketRequest WebSocketRequest { get; set; }

        public WebSocketResponse(WebSocketRequest webSocketRequest, WebSocketConnectionHandler handler)
        {
            WebSocketConnection = webSocketRequest?.WebSocketConnection;
            WebSocketRequest = webSocketRequest;
            Handler = handler;
        }

        public WebSocketSendBuilder CreateSend()
        {
            return Handler.CreateSend();
        }

        public async void SendMessageAsync(string message)
        {
            await Handler.SendMessageAsync(WebSocketConnection, message);
        }

        public async Task<WebSocketSendResult[]> SendObjectAsync(object obj)
        {
            if (Handler != null)
            {
                if (obj is ApiRequest)
                {
                    return await CreateSend().WithWebSocketConnection(WebSocketConnection).WithObject((ApiRequest)obj).Send();
                }
                else if (WebSocketRequest != null)
                {
                    var apiRequest = new ApiRequest(WebSocketRequest.Action, WebSocketRequest.Controller, WebSocketRequest.RequestId);
                    apiRequest.SetParam("model", obj);
                    return await CreateSend().WithWebSocketConnection(WebSocketConnection).WithApiRequest(apiRequest).Send();
                }
                else
                {
                    return await CreateSend().WithWebSocketConnection(WebSocketConnection).WithObject(obj).Send();
                }
            }
            return null;
        }
    }
}

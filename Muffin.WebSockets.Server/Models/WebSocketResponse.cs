using Muffin.Common.Api.WebSockets;
using Muffin.WebSockets.Server.Handler;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Models
{
    public class WebSocketResponse
    {
        protected WebSocketHandler Handler { get; set; }
        protected WebSocket Socket => SocketInfo?.WebSocket;
        protected SocketInfo SocketInfo { get; set; }
        internal WebSocketRequest WebSocketRequest { get; set; }

        public WebSocketResponse(WebSocketRequest webSocketRequest, WebSocketHandler handler)
        {
            SocketInfo = webSocketRequest?.SocketInfo;
            WebSocketRequest = webSocketRequest;
            Handler = handler;
        }

        public async void SendMessageAsync(string message)
        {
            await Handler.SendMessageAsync(Socket, message);
        }

        public async Task SendObjectAsync(object obj)
        {
            if (Handler != null)
            {
                if (obj is ApiRequest)
                {
                    await Handler.SendObjectAsync(SocketInfo, obj);
                }
                else if (WebSocketRequest != null)
                {
                    var apiRequest = new ApiRequest(WebSocketRequest.Action, WebSocketRequest.Controller, WebSocketRequest.RequestId);
                    apiRequest.SetParam("model", obj);
                    await Handler.SendObjectAsync(SocketInfo, apiRequest);
                }
                else
                {
                    await Handler.SendObjectAsync(SocketInfo, obj);
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Muffin.Common.Util;
using Muffin.WebSockets.Server.Handler;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Principal;

namespace Muffin.WebSockets.Server.Models
{
    public class WebSocketRequest
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string RequestId { get; set; }
        public IPrincipal Principal { get; set; }
        public string SocketId { get; set; }
        public WebSocket Socket => WebSocketConnection?.WebSocket;
        public WebSocketConnection WebSocketConnection { get; set; }
        //public WebSocketsReceiveResult Result { get; set; }
        public HttpContext Context { get; set; }
        public WebSocketConnectionHandler Handler { get; set; }

        public WebSocketRequest MakeGeneric(object model)
        {
            var modelType = model.GetType();
            var type = typeof(WebSocketsRequest<>)
                .MakeGenericType(modelType);
            var genericRequest = Activator.CreateInstance(type);
            PropertyHelper.CopyProperties(this, genericRequest);

            var pi = type.GetProperties().First(x => string.Equals(x.Name, "Model"));
            pi.SetValue(genericRequest, model);

            return genericRequest as WebSocketRequest;
        }
    }

    public class WebSocketsRequest<T> : WebSocketRequest
    {
        public T Model { get; set; }
    }
}

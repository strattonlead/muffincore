using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Common.Api.WebSockets;
using Muffin.WebSockets.Server.Invocation;
using Muffin.WebSockets.Server.Models;
//using Muffin.Common.Api.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Handler
{
    public class WebSocketConnectionHandler
    {
        #region Properties

        public WebSocketConnectionManager WebSocketConnectionManager { get; set; }
        protected readonly ILogger Logger;
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IServiceScopeFactory ServiceScopeFactory;
        protected readonly WebSocketHandlerEvents Events;

        #endregion

        #region Constructor

        public WebSocketConnectionHandler(WebSocketConnectionManager webSocketConnectionManager, IServiceProvider serviceProvider)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            Events = serviceProvider.GetService<WebSocketHandlerEvents>();
        }

        public WebSocketConnectionHandler(WebSocketConnectionManager WebSocketConnectionManager, ILogger logger, IServiceProvider serviceProvider)
            : this(WebSocketConnectionManager, serviceProvider)
        {
            Logger = logger;
        }

        #endregion

        #region Connection Handling

        public virtual WebSocketConnectionId OnConnected(WebSocket socket, HttpContext httpContext)
        {
            return WebSocketConnectionManager.AddWebSocket(socket, httpContext);
        }

        public virtual async Task<WebSocketConnectionId> OnDisconnected(WebSocket socket, HttpContext httpContext)
        {
            return await WebSocketConnectionManager.RemoveWebSocket(socket, httpContext);
        }

        public virtual async Task OnError(WebSocket socket, HttpContext httpContext)
        {
            await WebSocketConnectionManager.RemoveWebSocket(socket, httpContext);
        }

        public virtual async Task OnAborted(WebSocket socket, HttpContext httpContext)
        {
            await WebSocketConnectionManager.RemoveWebSocket(socket, httpContext);
        }

        public virtual bool IsConnected(string identity)
        {
            var webSocketConnections = WebSocketConnectionManager.Connections.GetWebSocketConnections(identity);
            return webSocketConnections != null && webSocketConnections.Length > 0;
        }

        public virtual async Task CloseConnection(string connectionId, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            await WebSocketConnectionManager.RemoveWebSocket(connectionId, null, closeStatus);
        }

        public virtual async Task CloseConnectionByIdentity(string identity, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            await WebSocketConnectionManager.RemoveWebSockets(identity, null, closeStatus);
        }

        #endregion

        #region Sending

        public async Task<WebSocketSendResult> SendMessageAsync(WebSocketConnection webSocketConnection, string message)
        {
            if (webSocketConnection == null)
            {
                Logger?.LogWarning($"SocketInfo is null! {(message != null ? (message.Length > 100 ? message.Substring(0, 100) : "") : "")}");
            }

            if (webSocketConnection.WebSocket == null)
            {
                Logger?.LogWarning($"Socket is null! {(message != null ? (message.Length > 100 ? message.Substring(0, 100) : "") : "")}");
                return null;
            }

            if (webSocketConnection.WebSocket.State != WebSocketState.Open)
            {
                Logger?.LogInformation($"Socket closed. Remove: {webSocketConnection.WebSocket}");
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(message);
            Logger?.LogInformation($"Send socket message with length: {bytes.Length} -> {(message?.Length > 100 ? message?.Substring(0, 99) : message)}");
            Events?.InvokeOnMessageSend(webSocketConnection, message);
            var sw = new Stopwatch();
            sw.Start();
            await webSocketConnection.WebSocket.SendAsync(buffer: new ArraySegment<byte>(array: bytes,
                                                                  offset: 0,
                                                                  count: bytes.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
            sw.Stop();
            Events?.InvokeOnMessageSent(webSocketConnection, message);

            if (webSocketConnection != null)
            {
                webSocketConnection.Statistics.LastSendDate = DateTime.UtcNow;
                webSocketConnection.Statistics.BytesSent += bytes.Length;
                return new WebSocketSendResult()
                {
                    ConnectionId = webSocketConnection.ConnectionId,
                    BytesSent = bytes.Length,
                    Identity = webSocketConnection.Identity,
                    Duration = sw.Elapsed
                };
            }

            return null;
        }

        public virtual async Task<WebSocketSendResult[]> Invoke(WebSocketSend webSocketSend)
        {
            var webSocketConnections = new List<WebSocketConnection>();

            if (webSocketSend.WebSocketConnections.Any())
            {
                webSocketConnections.AddRange(webSocketSend.WebSocketConnections);
            }

            if (webSocketSend.Identities.Any())
            {
                var items = WebSocketConnectionManager.Connections.Query.Where(x => webSocketSend.Identities.Contains(x.Identity)).ToArray();
                webSocketConnections.AddRange(items);
            }

            if (webSocketSend.WebSocketConnectionIds.Any())
            {
                var items = WebSocketConnectionManager.Connections.GetWebSocketConnections(webSocketSend.WebSocketConnectionIds);
                webSocketConnections.AddRange(items);
            }

            if (webSocketSend.Topics.Any())
            {
                var items = WebSocketConnectionManager.Connections.Query.Where(x => webSocketSend.Topics.Intersect(x.Topics).Any());
                webSocketConnections.AddRange(items);
            }

            if (webSocketSend.IgnoreWebSocketConnectionIds.Any())
            {
                var items = WebSocketConnectionManager.Connections.GetWebSocketConnections(webSocketSend.WebSocketConnectionIds);
                foreach (var item in items)
                {
                    webSocketConnections.Remove(item);
                }
            }

            webSocketConnections = webSocketConnections.Distinct().ToList();

            var sendTasks = webSocketConnections.Select(x => SendMessageAsync(x, webSocketSend.Message)).ToArray();
            return await Task.WhenAll(sendTasks);
        }

        public WebSocketSendBuilder CreateSend()
        {
            return new WebSocketSendBuilder(this);
        }

        #endregion

        #region Receiving

        public virtual async Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult result, byte[] buffer, HttpContext context)
        {
            var webSocketConnection = WebSocketConnectionManager.Connections.GetWebSocketConnection(webSocket);
            Events?.InvokeOnReceive(this, webSocket, webSocketConnection, buffer, context);
            if (webSocketConnection != null)
            {
                webSocketConnection.Statistics.LastReceiveDate = DateTime.UtcNow;
                webSocketConnection.Statistics.BytesReceived += buffer.Length;
            }
            await HandleApiReceive(webSocket, result, buffer, context);
            Events?.InvokeOnReceived(this, webSocket, webSocketConnection, buffer, context);
        }

        protected virtual void ApiRequestDetected(ApiRequest apiRequest, WebSocket webSocket, HttpContext httpContext) { }

        protected virtual async Task HandleApiReceive(WebSocket webSocket, WebSocketReceiveResult result, byte[] buffer, HttpContext context)
        {
            var data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            if (data?.Length >= 4)
            {
                if (data == "ping" || data == "Ping")
                {
                    return;
                }
            }

            WebSocketRequest webSocketRequest = null;
            WebSocketResponse webSocketResponse = null;
            try
            {
                ApiRequest apiRequest;
                try
                {
                    apiRequest = JsonConvert.DeserializeObject<ApiRequest>(data);
                    if (apiRequest.Params == null)
                    {
                        apiRequest.Params = new Dictionary<string, object>();
                    }

                    if (!string.IsNullOrWhiteSpace(apiRequest.Controller))
                    {
                        apiRequest.Controller = apiRequest.Controller.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(apiRequest.Action))
                    {
                        apiRequest.Action = apiRequest.Action.Trim();
                    }
                }
                catch
                {

                    Logger?.LogWarning($"Unable to deserialize request... Enable Trace to see request details. MessageType: {result.MessageType}");
                    if (data != null)
                    {
                        if (data.Length > 256)
                        {
                            Logger?.LogWarning($"Data: {data.Substring(0, 255)}");
                        }
                        else
                        {
                            Logger?.LogWarning($"Data: {data}");

                        }
                    }
                    else
                    {
                        Logger?.LogWarning($"No data");
                    }

                    return;
                }

                var webSocketConnection = WebSocketConnectionManager.Connections.GetWebSocketConnection(webSocket);
                webSocketRequest = new WebSocketRequest()
                {
                    Action = apiRequest.Action,
                    Controller = apiRequest.Controller,
                    RequestId = apiRequest.RequestId,
                    Principal = context.User,
                    SocketId = WebSocketConnectionManager.Connections.GetConnectionId(webSocket)?.ToString(),
                    Context = context,
                    WebSocketConnection = webSocketConnection,
                    Handler = this
                };

                if (apiRequest != null)
                {
                    ApiRequestDetected(apiRequest, webSocket, context);
                    Events?.InvokeOnApiReceived(this, webSocket, webSocketConnection, apiRequest, context);
                }

                webSocketResponse = new WebSocketResponse(webSocketRequest, this);

                if (!string.IsNullOrWhiteSpace(apiRequest.Controller) && !string.IsNullOrWhiteSpace(apiRequest.Action))
                {
                    try
                    {
                        await WebSocketControllerActivator.HandleWebSocketRequest(apiRequest, webSocketRequest, webSocketResponse, ServiceProvider);
                    }
                    catch (Exception e)
                    {
                        Logger?.LogError($"{e}");

                        var response = new ApiRequest()
                        {
                            RequestId = webSocketRequest?.RequestId,
                            Controller = webSocketRequest?.Controller,
                            Action = webSocketRequest?.Action
                        };

                        response.SetParam("error", e.Message);

                        await webSocketResponse.SendObjectAsync(response);
                    }
                }
            }
            catch (Exception transportError)
            {
                var response = new ApiRequest()
                {
                    RequestId = webSocketRequest?.RequestId,
                    Controller = webSocketRequest?.Controller,
                    Action = webSocketRequest?.Action
                };

                response.SetParam("error", transportError.Message);

                await webSocketResponse?.SendObjectAsync(response);
            }


        }

        protected virtual void InvokeApiEvent(MethodInfo mi, object target, object[] parameters, WebSocketRequest requestParameter, WebSocketResponse responseParameter)
        {
            mi.Invoke(target, parameters);
        }

        protected virtual void InvokeApiMethod(MethodInfo mi, object[] parameters, WebSocketRequest requestParameter, WebSocketResponse responseParameter)
        {
            mi.Invoke(this, parameters);
        }

        #endregion
    }

    public class WebSocketHandlerEvents
    {
        public event ReceiveEvent OnReceive;
        internal void InvokeOnReceive(WebSocketConnectionHandler sender, WebSocket webSocket, WebSocketConnection webSocketConnection, byte[] buffer, HttpContext context)
        {
            OnReceive?.Invoke(sender, new ReceiveEventArgs(webSocket, webSocketConnection, buffer, context));
        }
        public event ReceiveEvent OnReceived;
        internal void InvokeOnReceived(WebSocketConnectionHandler sender, WebSocket webSocket, WebSocketConnection webSocketConnection, byte[] buffer, HttpContext context)
        {
            OnReceived?.Invoke(sender, new ReceiveEventArgs(webSocket, webSocketConnection, buffer, context));
        }

        public event ApiReceiveEvent OnApiReceive;
        internal void InvokeOnApiReceived(WebSocketConnectionHandler sender, WebSocket webSocket, WebSocketConnection webSocketConnection, ApiRequest apiRequest, HttpContext context)
        {
            OnApiReceive?.Invoke(sender, new ReceiveApiReventArgs(webSocket, webSocketConnection, apiRequest, context));
        }

        public event SocketMessageEvent OnMessageSend;
        internal void InvokeOnMessageSend(WebSocketConnection webSocketConnection, string message)
        {
            if (webSocketConnection != null)
            {
                OnMessageSend?.Invoke(webSocketConnection, message);
            }
        }

        public event SocketMessageEvent OnMessageSent;
        internal void InvokeOnMessageSent(WebSocketConnection webSocketConnection, string message)
        {
            if (webSocketConnection != null)
            {
                OnMessageSent?.Invoke(webSocketConnection, message);
            }
        }
    }

    public class ReceiveEventArgs
    {
        public WebSocket WebSocket { get; set; }
        public WebSocketConnection WebSocketConnection { get; set; }
        public byte[] Buffer { get; set; }
        public HttpContext Context { get; set; }

        public ReceiveEventArgs(WebSocket webSocket, WebSocketConnection webSocketConnection, byte[] buffer, HttpContext context)
        {
            WebSocket = webSocket;
            WebSocketConnection = webSocketConnection;
            Buffer = buffer;
            Context = context;
        }
    }
    public delegate void ReceiveEvent(WebSocketConnectionHandler sender, ReceiveEventArgs args);

    public class ReceiveApiReventArgs
    {
        public WebSocket WebSocket { get; set; }
        public WebSocketConnection WebSocketConnection { get; set; }
        public ApiRequest ApiRequest { get; set; }
        public HttpContext Context { get; set; }

        public ReceiveApiReventArgs(WebSocket webSocket, WebSocketConnection webSocketConnection, ApiRequest apiRequest, HttpContext context)
        {
            WebSocket = webSocket;
            WebSocketConnection = webSocketConnection;
            ApiRequest = apiRequest;
            Context = context;
        }
    }
    public delegate void ApiReceiveEvent(WebSocketConnectionHandler sender, ReceiveApiReventArgs args);
    public delegate void SocketMessageEvent(WebSocketConnection webSocketConnection, string message);

    public class WebSocketSendResult
    {
        public WebSocketConnectionId ConnectionId { get; set; }
        public string Identity { get; set; }
        public TimeSpan Duration { get; set; }
        public long BytesSent { get; set; }
    }
}

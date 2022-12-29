using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Common.Api.WebSockets;
using Muffin.WebSockets.Server.Extensions;
using Muffin.WebSockets.Server.Models;
//using Muffin.Common.Api.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Handler
{
    public class WebSocketHandler
    {
        public WebSocketConnectionManager WebSocketConnectionManager { get; set; }
        //public SubscriptionChannelManager ChannelManager { get; protected set; }
        protected readonly ILogger Logger;
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IServiceScopeFactory ServiceScopeFactory;
        protected readonly WebSocketHandlerEvents Events;

        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager, IServiceProvider serviceProvider)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            Events = serviceProvider.GetService<WebSocketHandlerEvents>();
        }

        public WebSocketHandler(WebSocketConnectionManager WebSocketConnectionManager, ILogger logger, IServiceProvider serviceProvider)
            : this(WebSocketConnectionManager, serviceProvider)
        {
            Logger = logger;
        }

        public virtual WebSocketConnectionId OnConnected(WebSocket socket, HttpContext context)
        {
            return WebSocketConnectionManager.AddSocket(socket, context);
        }

        public virtual async Task<WebSocketConnectionId> OnDisconnected(WebSocket socket, HttpContext context)
        {
            return await WebSocketConnectionManager.RemoveSocket(socket, context);
        }

        public virtual async Task OnError(WebSocket socket, HttpContext context)
        {
            await WebSocketConnectionManager.RemoveSocket(socket, context);
        }

        public virtual async Task OnAborted(WebSocket socket, HttpContext context)
        {
            await WebSocketConnectionManager.RemoveSocket(socket, context);
        }

        public virtual bool IsConnected(string identity)
        {
            var socketInfos = WebSocketConnectionManager.Connections.GetSocketInfos(identity);
            return socketInfos != null && socketInfos.Length > 0;
        }

        #region Connection Handling

        public virtual async Task CloseConnection(string id, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            await WebSocketConnectionManager.RemoveSocket(id, closeStatus);
        }

        public virtual async Task CloseConnectionByIdentity(string identity, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            await WebSocketConnectionManager.RemoveSocketByIdentity(identity, closeStatus);
        }

        public virtual WebSocketConnectionId[] GetConnectionsByIdentity(string identity)
        {
            return WebSocketConnectionManager.Connections.GetSocketInfos(identity).Select(x => x.ConnectionId).ToArray();
        }

        public virtual SocketInfo[] GetSocketInfosByIdentity(string identity)
        {
            return WebSocketConnectionManager.Connections.GetSocketInfos(identity);
        }

        public SocketInfo[] GetSocketInfosByState(WebSocketState state)
        {
            return WebSocketConnectionManager.Connections
                .GetSocketInfos()
                .Where(x => x.WebSocket.State == state)
                .ToArray();
        }

        public async void RemoveSocket(SocketInfo socketInfo)
        {
            await WebSocketConnectionManager.RemoveSocket(socketInfo.SocketId.ToString(), WebSocketCloseStatus.NormalClosure);
        }

        #endregion

        #region Plaintext

        //public async Task<string> SendMessageAsync(WebSocket socket, string message)
        //{
        //    var socketInfo = WebSocketConnectionManager.Connections.GetSocketInfo(socket);
        //    return await SendMessageAsync(socketInfo, message);
        //}

        public async Task<string> SendMessageAsync(WebSocket webSocket, string message)
        {
            var socketInfo = WebSocketConnectionManager.Connections.GetSocketInfo(webSocket);
            if (socketInfo == null)
            {
                socketInfo = new SocketInfo() { WebSocket = webSocket };
            }
            return await SendMessageAsync(socketInfo, message);
        }

        public async Task<string> SendMessageAsync(SocketInfo socketInfo, string message)
        {
            if (socketInfo == null)
            {
                Logger?.LogWarning($"SocketInfo is null! {(message != null ? (message.Length > 100 ? message.Substring(0, 100) : "") : "")}");
            }

            if (socketInfo.WebSocket == null)
            {
                Logger?.LogWarning($"Socket is null! {(message != null ? (message.Length > 100 ? message.Substring(0, 100) : "") : "")}");
                return null;
            }

            if (socketInfo.WebSocket.State != WebSocketState.Open)
            {
                Logger?.LogInformation($"Socket closed. Remove: {socketInfo.WebSocket}");
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(message);
            Logger?.LogInformation($"Send socket message with length: {bytes.Length} -> {(message?.Length > 100 ? message?.Substring(0, 99) : message)}");
            Events?.InvokeOnMessageSend(socketInfo, message);
            await socketInfo.WebSocket.SendAsync(buffer: new ArraySegment<byte>(array: bytes,
                                                                  offset: 0,
                                                                  count: bytes.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
            Events?.InvokeOnMessageSent(socketInfo, message);

            if (socketInfo != null)
            {
                socketInfo.LastSendDate = DateTime.UtcNow;
                socketInfo.BytesSent += bytes.Length;
                return socketInfo.SocketId;
            }

            return null;
        }

        public async Task<string> SendMessageAsync(string socketId, string message)
        {
            var socketInfo = WebSocketConnectionManager.Connections.GetSocketInfo(socketId);
            if (socketInfo != null)
            {
                return await SendMessageAsync(socketInfo, message);
            }

            Logger?.LogWarning($"Socket is null! socketID: {socketId}");

            //var socketInfos = WebSocketConnectionManager.GetSocketInfosByIdentity(socketId);
            //if (socketInfos?.Any() ?? false)
            //{
            //    Logger?.LogWarning($"Socket is null! found socketInfos: {socketInfos.Length}");
            //    foreach (var socketInfo in socketInfos)
            //    {
            //        if (socketInfo.Socket != null)
            //        {
            //            return await SendMessageAsync(socketInfo.Socket, message);
            //        }
            //    }
            //}

            return null;
        }

        public async Task<string[]> SendMessageAsync(IEnumerable<string> socketIds, string message, IEnumerable<string> ignoreSocketIds = null)
        {
            var all = socketIds.ToArray();
            if (ignoreSocketIds != null)
            {
                all = all.Where(x => !ignoreSocketIds.Contains(x)).ToArray();
            }

            return await Task.WhenAll(all.Select(x => SendMessageAsync(x, message)));
        }

        public async Task<string[]> SendMessageAsync(IEnumerable<SocketInfo> sockets, string message, IEnumerable<string> ignoreSocketIds = null)
        {
            var receivers = sockets
                .GroupBy(x => x.SocketId)
                .Select(x => x.First())
                .ToArray();
            if (ignoreSocketIds != null)
            {
                receivers = receivers.Where(x => !ignoreSocketIds.Contains(x.SocketId)).ToArray();
            }

            return await Task.WhenAll(receivers.Select(x => SendMessageAsync(x, message)));
        }

        public async Task<string[]> SendMessageToAllAsync(string message, IEnumerable<string> ignoreSocketIds)
        {
            var socketInfos = WebSocketConnectionManager.Connections.GetSocketInfos();
            //var all = WebSocketConnectionManager.GetAll().ToArray();
            if (ignoreSocketIds != null)
            {
                socketInfos = socketInfos.Where(x => !ignoreSocketIds.Contains(x.SocketId)).ToArray();
                //all = all.Where(x => !ignoreSocketIds.Contains(x.Key)).ToArray();
            }

            return await Task.WhenAll(socketInfos
                .Where(x => x.WebSocket.State == WebSocketState.Open)
                .Select(x => SendMessageAsync(x, message)));
        }

        public async Task<string[]> SendMessageToPrincipalAsync(IPrincipal principal, string message)
        {
            var webSocketIds = WebSocketConnectionManager.Connections.GetSocketInfos(principal);
            //var WebSocketIds = WebSocketConnectionManager.GetIds(principal);
            return await SendMessageAsync(webSocketIds, message);
        }

        public async Task<string[]> SendMessageToPrincipalsAsync(IEnumerable<IPrincipal> principals, string message)
        {
            return (await Task.WhenAll(principals.Select(x => SendMessageToPrincipalAsync(x, message))))
                .SelectMany(x => x)
                .ToArray();
        }

        public async Task<string[]> SendMessageToIdentityAsync(string identity, string message, IEnumerable<string> ignoreSocketIds = null)
        {
            var socketInfos = WebSocketConnectionManager.Connections.GetSocketInfos(identity);

            if (ignoreSocketIds != null)
            {
                socketInfos = socketInfos.Where(x => !ignoreSocketIds.Contains(x.SocketId)).ToArray();
            }

            if (identity != null && socketInfos?.Length == 0)
            {
                Logger?.LogInformation($"SendMessageToIdentityAsync no web socketIds for identity {identity}");
            }

            return await SendMessageAsync(socketInfos, message);
        }

        public async Task<string[]> SendMessageToIdentitiesAsync(IEnumerable<string> identities, string message, IEnumerable<string> ignoreSocketIds = null)
        {
            return (await Task.WhenAll(identities.Select(x => SendMessageToIdentityAsync(x, message, ignoreSocketIds))))
                .SelectMany(x => x)
                .ToArray();
        }


        #endregion

        #region Objects

        public async Task<string> SendObjectAsync<T>(string socketId, T obj)
        {
            return await SendMessageAsync(socketId, _serialize(obj));
        }

        public async Task<string> SendObjectAsync<T>(SocketInfo socketInfo, T obj)
        {
            return await SendMessageAsync(socketInfo, _serialize(obj));
        }

        public async Task<string[]> SendObjectAsync<T>(IEnumerable<SocketInfo> socketInfos, T obj)
        {
            return await SendMessageAsync(socketInfos, _serialize(obj));
        }

        //public async Task<string> SendObjectAsync<T>(WebSocket socket, T obj)
        //{
        //    return await SendMessageAsync(socket, _serialize(obj));
        //}

        public async Task<string[]> SendObjectAsync<T>(IEnumerable<string> socketIds, T obj)
        {
            return await SendMessageAsync(socketIds, _serialize(obj));
        }

        public async Task<string[]> SendObjectAsync<T>(IEnumerable<string> socketIds, string action, T obj)
        {
            return await SendMessageAsync(socketIds, _serialize(obj, action));
        }

        public async Task<string[]> SendObjectAsync<T>(IEnumerable<SocketInfo> socketInfos, string action, T obj)
        {
            return await SendMessageAsync(socketInfos, _serialize(obj, action));
        }

        public async Task<string[]> SendObjectToAllAsync<T>(T obj, IEnumerable<string> ignoreSocketIds)
        {
            return await SendMessageToAllAsync(_serialize(obj), ignoreSocketIds);
        }

        public async Task<string[]> SendObjectToPrincipalAsync<T>(IPrincipal principal, string action, T obj)
        {
            var socketInfos = WebSocketConnectionManager.Connections.GetSocketInfos(principal);
            return await SendObjectAsync(socketInfos, action, obj);
        }

        public async Task<string[]> SendObjectToPrincipalAsync<T>(IPrincipal principal, T obj)
        {
            var socketInfos = WebSocketConnectionManager.Connections.GetSocketInfos(principal);
            return await SendObjectAsync(socketInfos, obj);
        }

        public async Task<string[]> SendObjectToPrincipalAsync<T>(IEnumerable<IPrincipal> principals, T obj)
        {
            return await SendMessageToPrincipalsAsync(principals, _serialize(obj));
        }

        public async Task<string[]> SendObjectToIdentity<T>(string identity, T obj, IEnumerable<string> ignoreSocketIds = null)
        {
            return await SendMessageToIdentityAsync(identity, _serialize(obj), ignoreSocketIds);
        }

        public async Task<string[]> SendObjectToIdentites<T>(IEnumerable<string> identities, T obj, IEnumerable<string> ignoreSocketIds = null)
        {
            return await SendMessageToIdentitiesAsync(identities, _serialize(obj), ignoreSocketIds);
        }

        //protected bool ShouldProcessAction(string socketId, string action)
        //{
        //    if (ChannelManager == null)
        //    {
        //        return true;
        //    }

        //    var socketInfo = WebSocketConnectionManager.GetSocketInfoBySocketId(socketId);
        //    if (socketInfo == null)
        //    {
        //        return true;
        //    }

        //    if (socketInfo.Subscriptions == null)
        //    {
        //        return false;
        //    }

        //    return socketInfo.Subscriptions.Contains(action);
        //}

        protected string _serialize<T>(T obj, string action = "", string controller = "", string requestId = "")
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Converters = { new FormatNumbersAsTextConverter() }
                };
                if (obj is ApiRequest && obj != null)
                {
                    var apiRequest = (ApiRequest)(object)obj;
                    if (!string.IsNullOrWhiteSpace(action))
                    {
                        apiRequest.Action = action;
                    }
                    if (!string.IsNullOrWhiteSpace(controller))
                    {
                        apiRequest.Controller = controller;
                    }
                    if (!string.IsNullOrWhiteSpace(requestId))
                    {
                        apiRequest.RequestId = requestId;
                    }
                    return JsonConvert.SerializeObject(apiRequest, settings);
                }
                var response = new ApiRequest(action, controller);
                response.RequestId = requestId;
                response.SetParam("model", obj);
                return JsonConvert.SerializeObject(response, settings);
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.LogError(e.ToString());
                }
                throw e;
            }
        }

        internal sealed class FormatNumbersAsTextConverter : JsonConverter
        {
            public override bool CanRead => false;
            public override bool CanWrite => true;
            public override bool CanConvert(Type type) => type == typeof(long) || type == typeof(long?);

            public override void WriteJson(
                JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value != null)
                {
                    if (value.GetType() == typeof(long))
                    {
                        long number = (long)value;
                        writer.WriteValue(number.ToString());
                    }
                    else if (value.GetType() == typeof(long?))
                    {
                        long? number = (long?)value;
                        writer.WriteValue(number.Value.ToString());
                    }
                }
                else
                {
                    writer.WriteValue((string)null);
                }
            }

            public override object ReadJson(
                JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        public virtual async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer, HttpContext context)
        {
            var socketInfo = WebSocketConnectionManager.Connections.GetSocketInfo(socket);
            Events?.InvokeOnReceive(this, socket, socketInfo, buffer, context);
            if (socketInfo != null)
            {
                socketInfo.LastReceiveDate = DateTime.UtcNow;
                socketInfo.BytesReceived += buffer.Length;
            }
            await HandleApiReceive(socket, result, buffer, context);
            Events?.InvokeOnReceived(this, socket, socketInfo, buffer, context);
        }

        protected virtual void ApiRequestDetected(ApiRequest apiRequest, WebSocket webSocket, HttpContext httpContext) { }

        protected virtual async Task HandleApiReceive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer, HttpContext context)
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

                var socketInfo = WebSocketConnectionManager.Connections.GetSocketInfo(socket);
                webSocketRequest = new WebSocketRequest()
                {
                    Action = apiRequest.Action,
                    Controller = apiRequest.Controller,
                    RequestId = apiRequest.RequestId,
                    Principal = context.User,
                    SocketId = WebSocketConnectionManager.Connections.GetConnectionId(socket)?.ToString(),
                    Context = context,
                    SocketInfo = socketInfo,
                    Handler = this
                };

                if (apiRequest != null)
                {
                    ApiRequestDetected(apiRequest, socket, context);
                    Events?.InvokeOnApiReceived(this, socket, socketInfo, apiRequest, context);
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
                        Logger?.LogError($"{e.ToString()}");

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

        protected Dictionary<Type, Type> InterfaceToTypeMappings { get; set; } = new Dictionary<Type, Type>();
        public void RegisterInterface<TInterface, TImplementation>()
            where TImplementation : new()
        {
            if (!typeof(TInterface).IsInterface)
            {
                throw new ArgumentException($"{nameof(TInterface)} must be an interface!");
            }
            if (typeof(TImplementation).IsInterface)
            {
                throw new ArgumentException($"{nameof(TImplementation)} must be a class!");
            }
            InterfaceToTypeMappings[typeof(TInterface)] = typeof(TImplementation);
        }

        public void RegisterInterfaces(IDictionary<Type, Type> typeMappings)
        {
            foreach (var pair in typeMappings)
            {
                InterfaceToTypeMappings[pair.Key] = pair.Value;
            }
        }

        #region 

        protected ApiRequest _dynamicMethodCall<T>(Expression<Action<T>> expression)
        {
            try
            {
                return ApiRequestExtensions.DynamicMethodCall(expression);
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.LogError(e.ToString());
                }
                throw e;
            }
        }

        public virtual async Task<string[]> InvokeAll<T>(Expression<Action<T>> expression, IEnumerable<string> ignoreSocketIds)
        {
            return await SendMessageToAllAsync(_serialize(_dynamicMethodCall(expression)), ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeAll(ApiRequest apiRequest, IEnumerable<string> ignoreSocketIds)
        {
            return await SendMessageToAllAsync(_serialize(apiRequest), ignoreSocketIds);
        }

        public virtual async Task<string> Invoke<T>(string socketId, Expression<Action<T>> expression)
        {
            return await SendMessageAsync(socketId, _serialize(_dynamicMethodCall(expression)));
        }

        public virtual async Task<string> Invoke<T>(string socketId, Expression<Action<T>> expression, string requestId)
        {
            return await SendMessageAsync(socketId, _serialize(_dynamicMethodCall(expression), "", "", requestId));
        }

        public virtual async Task<string> Invoke<T>(SocketInfo socketInfo, Expression<Action<T>> expression)
        {
            return await SendMessageAsync(socketInfo, _serialize(_dynamicMethodCall(expression)));
        }

        public virtual async Task<string> Invoke<T>(SocketInfo socketInfo, Expression<Action<T>> expression, string requestId)
        {
            return await SendMessageAsync(socketInfo, _serialize(_dynamicMethodCall(expression), "", "", requestId));
        }

        public virtual async Task<string[]> Invoke<T>(IPrincipal principal, Expression<Action<T>> expression)
        {
            return await SendMessageToPrincipalAsync(principal, _serialize(_dynamicMethodCall(expression)));
        }

        public virtual async Task<string[]> Invoke<T>(IEnumerable<IPrincipal> principals, Expression<Action<T>> expression)
        {
            return await SendMessageToPrincipalsAsync(principals, _serialize(_dynamicMethodCall(expression)));
        }
        public virtual async Task<string[]> InvokeWithIdentity<T>(string identity, string requestId, Expression<Action<T>> expression, IEnumerable<string> ignoreSocketIds = null)
        {
            var apiRequest = _dynamicMethodCall(expression);
            apiRequest.RequestId = requestId;
            return await InvokeWithIdentity(identity, apiRequest, ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeWithIdentity<T>(string identity, Expression<Action<T>> expression, IEnumerable<string> ignoreSocketIds = null)
        {
            var apiRequest = _dynamicMethodCall(expression);
            return await InvokeWithIdentity(identity, apiRequest, ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeWithIdentity(string identity, ApiRequest apiRequest, IEnumerable<string> ignoreSocketIds = null)
        {
            return await SendMessageToIdentityAsync(identity, _serialize(apiRequest), ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeWithIdentities<T>(IEnumerable<string> identities, Expression<Action<T>> expression, IEnumerable<string> ignoreSocketIds = null)
        {
            return await SendMessageToIdentitiesAsync(identities, _serialize(_dynamicMethodCall(expression)), ignoreSocketIds);
        }

        #endregion

        #region Subscriptions

        public virtual async Task<string[]> SendMessageToSubscriptionAsync(string channelId, string message, IEnumerable<string> ignoreSocketIds = null)
        {
            return await SendMessageToSubscriptionsAsync(new string[] { channelId }, message, ignoreSocketIds);
        }

        public virtual async Task<string[]> SendMessageToSubscriptionsAsync(IEnumerable<string> channelIds, string message, IEnumerable<string> ignoreSocketIds = null)
        {
            var socketInfos = WebSocketConnectionManager.Connections.GetSocketInfosByChannelIds(channelIds);
            //var webSocketIds = WebSocketConnectionManager.GetSocketIdsByChannelIds(channelIds);
            return await SendMessageAsync(socketInfos, message, ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeSubscriptionAsync<T>(string channelId, Expression<Action<T>> expression, IEnumerable<string> ignoreSocketIds = null)
        {
            return await InvokeSubscriptionsAsync(new string[] { channelId }, expression, ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeSubscriptionAsync(string channelId, ApiRequest apiRequest, IEnumerable<string> ignoreSocketIds = null)
        {
            return await InvokeSubscriptionsAsync(new string[] { channelId }, apiRequest, ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeSubscriptionsAsync<T>(IEnumerable<string> channelIds, Expression<Action<T>> expression, IEnumerable<string> ignoreSocketIds = null)
        {
            if (channelIds == null)
            {
                return null;
            }

            var apiRequest = _dynamicMethodCall(expression);
            return await InvokeSubscriptionsAsync(channelIds, apiRequest, ignoreSocketIds);
        }

        public virtual async Task<string[]> InvokeSubscriptionsAsync(IEnumerable<string> channelIds, ApiRequest apiRequest, IEnumerable<string> ignoreSocketIds = null)
        {
            if (channelIds == null)
            {
                return null;
            }

            var result = new List<string>();
            foreach (var channelId in channelIds)
            {
                apiRequest.ChannelId = channelId;
                var message = _serialize(apiRequest);
                var socketIds = await SendMessageToSubscriptionAsync(channelId, message, ignoreSocketIds);
                if (socketIds != null)
                {
                    result.AddRange(socketIds);
                }
            }

            return result.ToArray();
        }

        #endregion
    }

    public class WebSocketHandlerEvents
    {
        public event ReceiveEvent OnReceive;
        internal void InvokeOnReceive(WebSocketHandler sender, WebSocket webSocket, SocketInfo socketInfo, byte[] buffer, HttpContext context)
        {
            OnReceive?.Invoke(sender, new ReceiveEventArgs(webSocket, socketInfo, buffer, context));
        }
        public event ReceiveEvent OnReceived;
        internal void InvokeOnReceived(WebSocketHandler sender, WebSocket webSocket, SocketInfo socketInfo, byte[] buffer, HttpContext context)
        {
            OnReceived?.Invoke(sender, new ReceiveEventArgs(webSocket, socketInfo, buffer, context));
        }

        public event ApiReceiveEvent OnApiReceive;
        internal void InvokeOnApiReceived(WebSocketHandler sender, WebSocket webSocket, SocketInfo socketInfo, ApiRequest apiRequest, HttpContext context)
        {
            OnApiReceive?.Invoke(sender, new ReceiveApiReventArgs(webSocket, socketInfo, apiRequest, context));
        }

        public event SocketMessageEvent OnMessageSend;
        internal void InvokeOnMessageSend(SocketInfo socketInfo, string message)
        {
            if (socketInfo != null)
            {
                OnMessageSend?.Invoke(socketInfo, message);
            }
        }

        public event SocketMessageEvent OnMessageSent;
        internal void InvokeOnMessageSent(SocketInfo socketInfo, string message)
        {
            if (socketInfo != null)
            {
                OnMessageSent?.Invoke(socketInfo, message);
            }
        }
    }

    public class ReceiveEventArgs
    {
        public WebSocket WebSocket { get; set; }
        public SocketInfo SocketInfo { get; set; }
        public byte[] Buffer { get; set; }
        public HttpContext Context { get; set; }

        public ReceiveEventArgs(WebSocket webSocket, SocketInfo socketInfo, byte[] buffer, HttpContext context)
        {
            WebSocket = webSocket;
            SocketInfo = socketInfo;
            Buffer = buffer;
            Context = context;
        }
    }
    public delegate void ReceiveEvent(WebSocketHandler sender, ReceiveEventArgs args);

    public class ReceiveApiReventArgs
    {
        public WebSocket WebSocket { get; set; }
        public SocketInfo SocketInfo { get; set; }
        public ApiRequest ApiRequest { get; set; }
        public HttpContext Context { get; set; }

        public ReceiveApiReventArgs(WebSocket webSocket, SocketInfo socketInfo, ApiRequest apiRequest, HttpContext context)
        {
            WebSocket = webSocket;
            SocketInfo = socketInfo;
            ApiRequest = apiRequest;
            Context = context;
        }
    }
    public delegate void ApiReceiveEvent(WebSocketHandler sender, ReceiveApiReventArgs args);
    public delegate void SocketMessageEvent(SocketInfo socketInfo, string message);

}

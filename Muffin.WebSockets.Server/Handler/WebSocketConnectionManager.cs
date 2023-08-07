using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muffin.Common.Util;
using Muffin.WebSockets.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Handler
{
    public class WebSocketConnections
    {
        private Dictionary<long, Dictionary<string, Dictionary<WebSocketConnectionId, SocketInfo>>> _socketsByTenantId => _socketsByConnectionId.Values.GroupBy(x => x.TenantId).ToDictionary(x => x.Key ?? NO_TENANCY, x => x.GroupBy(y => y.Identity).ToDictionary(y => y.Key, y => y.ToDictionary(z => z.ConnectionId)));
        private Dictionary<WebSocketConnectionId, SocketInfo> _socketsByConnectionId => _socketsByIdentity.Values.Merge();
        private Dictionary<string, Dictionary<WebSocketConnectionId, SocketInfo>> _socketsByIdentity = new Dictionary<string, Dictionary<WebSocketConnectionId, SocketInfo>>();
        private const string ANONYMOUS_IDENTITY = "anonymous";
        private const long NO_TENANCY = long.MinValue;

        public void AddWebSocket(WebSocketConnectionId connectionId, SocketInfo socketInfo)
        {
            var identity = connectionId.Identity;
            if (connectionId.IsAnonymous)
            {
                identity = ANONYMOUS_IDENTITY;
            }

            if (!_socketsByIdentity.TryGetValue(identity, out var socketInfos))
            {
                socketInfos = new Dictionary<WebSocketConnectionId, SocketInfo>();
                _socketsByIdentity.Add(identity, socketInfos);
            }

            socketInfos[connectionId] = socketInfo;
        }

        public void RemoveWebSocket(WebSocketConnectionId connectionId)
        {
            var identity = connectionId.Identity;
            if (connectionId.IsAnonymous)
            {
                identity = ANONYMOUS_IDENTITY;
            }

            if (_socketsByIdentity.TryGetValue(identity, out var socketInfos))
            {
                socketInfos.Remove(connectionId);
            }
        }

        public SocketInfo GetSocketInfo(WebSocket webSocket)
        {
            var connectionId = GetConnectionId(webSocket);
            return GetSocketInfo(connectionId);
        }

        public SocketInfo GetSocketInfo(WebSocketConnectionId connectionId)
        {
            if (connectionId == null)
            {
                return null;
            }

            _socketsByConnectionId.TryGetValue(connectionId, out var socketInfo);
            return socketInfo;
        }

        public WebSocket GetWebSocket(WebSocketConnectionId connectionId)
        {
            return GetSocketInfo(connectionId)?.WebSocket;
        }

        public SocketInfo[] GetSocketInfos(IEnumerable<string> identities)
        {
            return identities?
                .Where(identity => _socketsByIdentity.ContainsKey(identity))?
                .Select(identity => _socketsByIdentity[identity])?
                .SelectMany(x => x.Values)?
                .ToArray() ?? new SocketInfo[0];
        }

        public SocketInfo[] GetSocketInfos(string identity)
        {
            _socketsByIdentity.TryGetValue(identity, out var sockets);
            return sockets?.Select(x => x.Value)?.ToArray() ?? new SocketInfo[0];
        }

        public SocketInfo[] GetSocketInfos(IPrincipal principal)
        {
            return GetSocketInfos(principal.Identity.Name);
        }

        public SocketInfo[] GetSocketInfos()
        {
            return _socketsByIdentity.Values.SelectMany(x => x.Values).ToArray();
        }

        public Dictionary<string, WebSocket> GetAllWebSocketsDictionary()
        {
            return GetSocketInfos().ToDictionary(x => x.SocketId, x => x.WebSocket);
        }

        public WebSocket[] GetAllWebSockets()
        {
            return _socketsByConnectionId.Values.Select(x => x.WebSocket).ToArray();
        }

        public WebSocketConnectionId GetConnectionId(WebSocket webSocket)
        {
            return _socketsByConnectionId.Values.FirstOrDefault(x => x.WebSocket == webSocket)?.ConnectionId;
        }

        public SocketInfo[] GetSocketInfosByChannelIds(IEnumerable<string> channelIds)
        {
            return channelIds.SelectMany(x => GetSocketInfosByChannelId(x))
                .Distinct()
                .ToArray();
        }

        public SocketInfo[] GetSocketInfosByChannelId(string channelId)
        {
            return _socketsByConnectionId.Values
                .Where(x => x.Subscriptions != null && x.Subscriptions.Contains(channelId))
                .Distinct()
                .ToArray();
        }

        public Dictionary<string, Dictionary<WebSocketConnectionId, SocketInfo>> GetSocketInfos(long? tenantId)
        {
            if (_socketsByTenantId.TryGetValue(tenantId ?? NO_TENANCY, out var result))
            {
                return result;
            }
            return new Dictionary<string, Dictionary<WebSocketConnectionId, SocketInfo>>();
        }

        public void Clear()
        {
            _socketsByIdentity.Clear();
        }

        public Dictionary<string, Dictionary<WebSocketConnectionId, SocketInfo>> SocketsByIdentity => _socketsByIdentity;
        public Dictionary<WebSocketConnectionId, SocketInfo> SocketsByConnectionId => _socketsByConnectionId;
    }

    public class WebSocketConnectionManager
    {
        public readonly WebSocketConnections Connections = new WebSocketConnections();

        private readonly WebSocketManagerOptions Options;
        private readonly IServiceProvider ServiceProvider;
        private readonly WebSocketConnectionManagerEvents Events;
        private readonly IHostApplicationLifetime ApplicationLifetime;

        public WebSocketConnectionManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Options = serviceProvider.GetService<WebSocketManagerOptions>();
            Events = serviceProvider.GetService<WebSocketConnectionManagerEvents>();
            ApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();

            ApplicationLifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    var sockets = Connections.GetAllWebSockets();
                    var closeTasks = sockets.Select(socket =>
                    {
                        return socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                        statusDescription: "Closed by the WebSocketManager",
                                        cancellationToken: CancellationToken.None);
                    }).ToArray();
                    Connections.Clear();
                    Task.WhenAll(closeTasks).Wait(TimeSpan.FromSeconds(5));
                }
                catch { }
            });
        }

        //public WebSocket GetSocketById(string id)
        //{
        //    return GetSocketById((WebSocketConnectionId)id);
        //}

        //public WebSocket GetSocketById(WebSocketConnectionId id)
        //{
        //    return Connections.GetSocketInfo(id)?.Socket;
        //}

        //public Dictionary<WebSocketConnectionId, WebSocket> GetAllSockets()
        //{
        //    return _socketsByConnectionId.ToDictionary(x => x.Key, x => x.Value.Socket);
        //}

        //[Obsolete]
        //public Dictionary<string, WebSocket> GetAll()
        //{
        //    return GetAllSockets().ToDictionary(x => x.Key.ToString(), x => x.Value);
        //}

        //public SocketInfo[] GetAllSocketInfos()
        //{
        //    return _socketsByConnectionId.Select(x => x.Value).ToArray();
        //}

        //public string GetPrincipalName(WebSocket socket)
        //{
        //    var socketId = GetId(socket);
        //    return _socketIds.Where(x => x.Value.Contains(socketId))
        //        .Select(x => x.Key) // Hier sollte es tatsächlich nur einen geben...
        //        .FirstOrDefault();
        //}

        //public string GetId(WebSocket socket)
        //{
        //    var entry = _sockets.FirstOrDefault(p => p.Value.Socket == socket);
        //    if (entry.Value != null)
        //    {
        //        return entry.Key;
        //    }
        //    return null;
        //}

        //public SocketInfo GetSocketInfoBySocket(WebSocket socket)
        //{
        //    return _sockets.FirstOrDefault(p => p.Value.Socket == socket).Value;
        //}

        //public SocketInfo GetSocketInfoBySocketId(string socketId)
        //{
        //    _socketsByConnectionId.TryGetValue(socketId, out var socketInfo);
        //    return socketInfo;
        //}

        //public string[] GetIds(IPrincipal principal)
        //{
        //    return GetSocketIdsByIdentity(principal.Identity.Name);
        //}

        //public string[] GetSocketIdsByChannelIds(IEnumerable<string> channelIds)
        //{
        //    return channelIds.SelectMany(x => GetSocketIdsByChannelId(x))
        //        .Distinct()
        //        .ToArray();
        //}

        //public string[] GetSocketIdsByChannelId(string channelId)
        //{
        //    return _socketsByConnectionId.Values
        //        .Where(x => x.Subscriptions != null && x.Subscriptions.Contains(channelId))
        //        .Select(x => x.SocketId)
        //        .Distinct()
        //        .ToArray();
        //}

        //public string[] GetSocketIdsByIdentity(string identity)
        //{
        //    _socketsByIdentity.TryGetValue(identity, out var socketIdsByIdentity);
        //    return socketIdsByIdentity?.Values.Select(x => x.SocketId)?.ToArray() ?? new string[0];
        //}

        //public SocketInfo[] GetSocketInfosByIdentity(string identity)
        //{
        //    _socketsByIdentity.TryGetValue(identity, out var socketInfosByIdentity);
        //    return socketInfosByIdentity?.Values?.ToArray() ?? new SocketInfo[0];
        //}

        //public SocketInfo[] GetSocketInfosByIdentities(IEnumerable<string> identities)
        //{
        //    return identities?
        //        .Where(x => _socketsByIdentity.ContainsKey(x))?
        //        .SelectMany(x => _socketsByIdentity[x].Values)?
        //        .ToArray()
        //        ?? new SocketInfo[0];
        //}

        public WebSocketConnectionId AddSocket(WebSocket socket, HttpContext context)
        {
            var connectionId = Connections.GetConnectionId(socket);
            if (connectionId != null)
            {
                return connectionId;
            }

            connectionId = WebSocketConnectionId.NewConnectionId(context);
            var socketInfo = new SocketInfo()
            {
                ConnectionId = connectionId,
                WebSocket = socket
            };
            context.Features.Set(new WebSocketFeature(socketInfo));



            var identity = Options?.GetSocketIdentity?.Invoke(ServiceProvider, context);

            if (identity == null)
            {
                identity = context?.User?.Identity?.Name;
            }

            if (identity != null /*&& context.User.Identity.IsAuthenticated*/)
            {
                connectionId.Identity = identity;
                socketInfo.Identity = identity;
                socketInfo.Roles = ((ClaimsIdentity)context?.User?.Identity)?
                    .Claims?
                    .Where(c => c.Type == ClaimTypes.Role)?
                    .Select(c => c.Value);
            }

            Connections.AddWebSocket(connectionId, socketInfo);
            Events?.InvokeOnSocketAdded(socketInfo, context);

            return connectionId;
        }

        public async Task RemoveSocketByIdentity(string identity, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            var socketInfos = Connections.GetSocketInfos(identity);
            var tasks = socketInfos.Select(x => RemoveSocket(x.SocketId, closeStatus));
            await Task.WhenAll(tasks);
        }

        public async Task<WebSocketConnectionId> RemoveSocket(WebSocketConnectionId connectionId, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            if (connectionId == null)
            {
                return null;
            }

            var socketInfo = Connections.GetSocketInfo(connectionId);
            if (socketInfo != null)
            {
                Connections.RemoveWebSocket(connectionId);
                Events?.InvokeOnSocketRemoved(socketInfo);
            }

            if (socketInfo.WebSocket.State == WebSocketState.Open)
            {
                await socketInfo.WebSocket.CloseAsync(closeStatus: closeStatus,
                                                        statusDescription: "Closed by the WebSocketManager",
                                                        cancellationToken: CancellationToken.None);
            }

            return connectionId;
        }

        public async Task<WebSocketConnectionId> RemoveSocket(WebSocket webSocket, HttpContext context)
        {
            var connectionId = Connections.GetConnectionId(webSocket);
            return await RemoveSocket(connectionId);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muffin.WebSockets.Server.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        #region Properties

        private readonly Dictionary<WebSocketConnectionId, WebSocketConnection> _webSocketConnections = new Dictionary<WebSocketConnectionId, WebSocketConnection>();
        public IEnumerable<WebSocketConnection> Query => _webSocketConnections.Values;

        public Dictionary<WebSocketConnectionId, WebSocketConnection> ByTenant(long? tenantId)
        {
            if (!tenantId.HasValue)
            {
                return _webSocketConnections;
            }

            return _webSocketConnections.Values.Where(x => x.TenantId == tenantId).ToDictionary(x => x.ConnectionId);
        }

        public const string ANONYMOUS_IDENTITY = "anonymous";
        public readonly long? NO_TENANCY = null;
        private readonly IServiceProvider ServiceProvider;
        private readonly WebSocketManagerOptions Options;

        #endregion

        #region Constructor

        public WebSocketConnections(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Options = serviceProvider.GetService<WebSocketManagerOptions>();
        }

        #endregion

        public WebSocketConnection AddWebSocket(HttpContext httpContext, WebSocket webSocket)
        {
            var tenantId = Options.GetTenantId?.Invoke(ServiceProvider, httpContext);
            var connectionId = WebSocketConnectionId.NewConnectionId(httpContext, tenantId);
            var webSocketConnection = new WebSocketConnection(connectionId, webSocket);
            var webSocketFeature = new WebSocketFeature(webSocketConnection);

            httpContext.Features.Set(webSocketFeature);

            var identity = Options?.GetSocketIdentity?.Invoke(ServiceProvider, httpContext);

            if (identity == null)
            {
                identity = httpContext?.User?.Identity?.Name;
            }

            if (identity != null /*&& context.User.Identity.IsAuthenticated*/)
            {
                connectionId.Identity = identity;
                webSocketConnection.Roles = ((ClaimsIdentity)httpContext?.User?.Identity)?
                    .Claims?
                    .Where(c => c.Type == ClaimTypes.Role)?
                    .Select(c => c.Value)
                    .ToArray();
            }

            _webSocketConnections.Add(connectionId, webSocketConnection);
            return webSocketConnection;
        }

        public WebSocketConnection RemoveWebSocket(WebSocketConnectionId connectionId)
        {
            if (_webSocketConnections.TryGetValue(connectionId, out var webSocketConnection))
            {
                _webSocketConnections.Remove(connectionId);
            }
            return webSocketConnection;
        }

        public WebSocketConnection GetWebSocketConnection(WebSocket webSocket)
        {
            var connectionId = GetConnectionId(webSocket);
            return GetWebSocketConnection(connectionId);
        }

        public WebSocketConnection[] GetWebSocketConnections(IEnumerable<WebSocketConnectionId> connectionIds)
        {
            return connectionIds.Select(x => GetWebSocketConnection(x)).Where(x => x != null).ToArray();
        }

        public WebSocketConnection GetWebSocketConnection(WebSocketConnectionId connectionId)
        {
            if (connectionId == null)
            {
                return null;
            }

            _webSocketConnections.TryGetValue(connectionId, out var socketInfo);
            return socketInfo;
        }

        public WebSocket GetWebSocket(WebSocketConnectionId connectionId)
        {
            return GetWebSocketConnection(connectionId)?.WebSocket;
        }

        public WebSocketConnection[] GetWebSocketConnections(IEnumerable<string> identities)
        {
            return identities?
                .SelectMany(x => GetWebSocketConnections(x))
                .ToArray();
        }

        public WebSocketConnection[] GetWebSocketConnections(string identity)
        {
            return _webSocketConnections.Values.Where(x => x.Identity == identity).ToArray();
        }

        public WebSocketConnection[] GetWebSocketConnections(IPrincipal principal)
        {
            return GetWebSocketConnections(principal.Identity.Name);
        }

        public WebSocket[] GetWebSockets()
        {
            return _webSocketConnections.Values.Select(x => x.WebSocket).ToArray();
        }

        public WebSocketConnectionId GetConnectionId(WebSocket webSocket)
        {
            return _webSocketConnections.Values.FirstOrDefault(x => x.WebSocket == webSocket)?.ConnectionId;
        }

        public Dictionary<string, WebSocketConnection[]> GetWebSocketConnections(long? tenantId)
        {
            return _webSocketConnections.Values.Where(x => x.TenantId == tenantId).GroupBy(x => x.Identity).ToDictionary(x => x.Key, x => x.ToArray());
        }

        public void Clear()
        {
            _webSocketConnections.Clear();
        }

        public bool IsConnected(WebSocketConnectionId connectionId)
        {
            return _webSocketConnections.ContainsKey(connectionId);
        }
    }

    public class WebSocketConnection : IEquatable<WebSocketConnection>, IEqualityComparer<WebSocketConnection>
    {
        public string Id => ConnectionId?.ToString();
        public WebSocketConnectionId ConnectionId { get; set; }
        public string Identity => ConnectionId.Identity;
        public string RandomId => ConnectionId.RandomId;
        public bool IsAnonymous => ConnectionId.IsAnonymous;
        public long? TenantId => ConnectionId.TenantId;
        public string[] Roles { get; set; }
        public List<string> Topics { get; private set; } = new List<string>();

        public WebSocket WebSocket { get; set; }
        public WebSocketConnectionStatistics Statistics { get; private set; } = new WebSocketConnectionStatistics();

        public WebSocketConnection(WebSocketConnectionId connectionId, WebSocket webSocket)
        {
            ConnectionId = connectionId;
            WebSocket = webSocket;
        }

        public bool Equals(WebSocketConnection other)
        {
            return Id == other?.Id;
        }

        public bool Equals(WebSocketConnection x, WebSocketConnection y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode([DisallowNull] WebSocketConnection obj)
        {
            return obj.Id.GetHashCode();
        }

        public void Subscribe(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return;
            }

            if (Topics.Contains(topic))
            {
                return;
            }

            Topics.Add(topic);
        }

        public void Unsubscribe(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return;
            }

            if (Topics.Contains(topic))
            {
                Topics.Remove(topic);
            }
        }

        public void UnsubscribeAll()
        {
            Topics = new List<string>();
        }
    }

    public class WebSocketConnectionStatistics
    {
        public DateTime? CreatedDateUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastSendDate { get; set; }
        public DateTime? LastReceiveDate { get; set; }

        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
    }

    public class WebSocketConnectionManager
    {
        public readonly WebSocketConnections Connections;

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
            Connections = new WebSocketConnections(serviceProvider);

            ApplicationLifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    var sockets = Connections.GetWebSockets();
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

        public WebSocketConnectionId AddWebSocket(WebSocket webSocket, HttpContext httpContext)
        {
            var webSocketConnection = Connections.AddWebSocket(httpContext, webSocket);
            Events?.InvokeOnSocketAdded(webSocketConnection, httpContext);

            return webSocketConnection.ConnectionId;
        }

        public async Task RemoveWebSockets(string identity, HttpContext httpContext, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            var socketInfos = Connections.GetWebSocketConnections(identity);
            var tasks = socketInfos.Select(x => RemoveWebSocket(x.ConnectionId, httpContext, closeStatus));
            await Task.WhenAll(tasks);
        }

        public async Task<WebSocketConnectionId> RemoveWebSocket(WebSocketConnectionId connectionId, HttpContext httpContext, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            if (connectionId == null)
            {
                return null;
            }

            var webSocketConnection = Connections.GetWebSocketConnection(connectionId);
            if (webSocketConnection != null)
            {
                Connections.RemoveWebSocket(connectionId);
                Events?.InvokeOnSocketRemoved(webSocketConnection, httpContext);
            }

            if (webSocketConnection.WebSocket.State != WebSocketState.Closed)
            {
                await webSocketConnection.WebSocket.CloseAsync(closeStatus: closeStatus,
                                                        statusDescription: "Closed by the WebSocketManager",
                                                        cancellationToken: CancellationToken.None);
            }

            return connectionId;
        }

        public async Task<WebSocketConnectionId> RemoveWebSocket(WebSocket webSocket, HttpContext httpContext)
        {
            var connectionId = Connections.GetConnectionId(webSocket);
            return await RemoveWebSocket(connectionId, httpContext);
        }
    }

    public class WebSocketConnectionManagerEvents
    {
        internal void InvokeOnSocketAdded(WebSocketConnection webSocketConnection, HttpContext httpContext)
        {
            if (webSocketConnection != null)
            {
                OnSocketAdded?.Invoke(webSocketConnection, httpContext);
            }
        }

        internal void InvokeOnSocketRemoved(WebSocketConnection webSocketConnection, HttpContext httpContext)
        {
            if (webSocketConnection != null)
            {
                OnSocketRemoved?.Invoke(webSocketConnection, httpContext);
            }
        }

        public event SocketEvent OnSocketAdded;
        public event SocketEvent OnSocketRemoved;

    }

    public delegate void SocketEvent(WebSocketConnection webSocketConnection, HttpContext httpContext);

    public static class WebSocketConnectionManagerEventsHelper
    {
        public static void AddWebSocketConnectionManagerEvents(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketConnectionManagerEvents>();
        }
    }
}

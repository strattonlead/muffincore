namespace Muffin.WebSockets.Server.Handler
{
    //public class WebSocketConnectionManager
    //{
    //    private ConcurrentDictionary<string, SocketInfo> _sockets = new ConcurrentDictionary<string, SocketInfo>();
    //    private ConcurrentDictionary<string, List<string>> _socketIds = new ConcurrentDictionary<string, List<string>>();
    //    private readonly WebSocketManagerOptions Options;
    //    private readonly IServiceProvider ServiceProvider;
    //    private readonly WebSocketConnectionManagerEvents Events;
    //    private readonly IHostApplicationLifetime ApplicationLifetime;

    //    public WebSocketConnectionManager(IServiceProvider serviceProvider)
    //    {
    //        ServiceProvider = serviceProvider;
    //        Options = serviceProvider.GetService<WebSocketManagerOptions>();
    //        Events = serviceProvider.GetService<WebSocketConnectionManagerEvents>();
    //        ApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();

    //        ApplicationLifetime.ApplicationStopping.Register(() =>
    //        {
    //            try
    //            {
    //                var sockets = GetAll().Values;
    //                var closeTasks = sockets.Select(socket =>
    //                {
    //                    return socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
    //                                    statusDescription: "Closed by the WebSocketManager",
    //                                    cancellationToken: CancellationToken.None);
    //                }).ToArray();
    //                _sockets.Clear();
    //                _socketIds.Clear();
    //                Task.WhenAll(closeTasks).Wait(TimeSpan.FromSeconds(10));
    //            }
    //            catch { }
    //        });
    //    }

    //    public WebSocket GetSocketById(string id)
    //    {
    //        var socketInfo = _sockets.FirstOrDefault(p => p.Key == id);
    //        if (socketInfo.Value != null)
    //            return socketInfo.Value.Socket;
    //        return null;
    //    }

    //    public ConcurrentDictionary<string, WebSocket> GetAll()
    //    {
    //        return new ConcurrentDictionary<string, WebSocket>(_sockets.ToDictionary(x => x.Key, x => x.Value.Socket));
    //    }

    //    public SocketInfo[] GetAllSocketInfos()
    //    {
    //        return _sockets.Select(x => x.Value).ToArray();
    //    }

    //    public string GetPrincipalName(WebSocket socket)
    //    {
    //        var socketId = GetId(socket);
    //        return _socketIds.Where(x => x.Value.Contains(socketId))
    //            .Select(x => x.Key) // Hier sollte es tatsächlich nur einen geben...
    //            .FirstOrDefault();
    //    }

    //    public string GetId(WebSocket socket)
    //    {
    //        var entry = _sockets.FirstOrDefault(p => p.Value.Socket == socket);
    //        if (entry.Value != null)
    //        {
    //            return entry.Key;
    //        }
    //        return null;
    //    }

    //    public SocketInfo GetSocketInfoBySocket(WebSocket socket)
    //    {
    //        return _sockets.FirstOrDefault(p => p.Value.Socket == socket).Value;
    //    }

    //    public SocketInfo GetSocketInfoBySocketId(string socketId)
    //    {
    //        if (socketId == null)
    //        {
    //            return null;
    //        }
    //        return _sockets.FirstOrDefault(p => string.Equals(p.Value.SocketId, socketId)).Value;
    //    }

    //    public string[] GetIds(IPrincipal principal)
    //    {
    //        return GetSocketIdsByIdentity(principal.Identity.Name);
    //    }

    //    public string[] GetSocketIdsByChannelIds(IEnumerable<string> channelIds)
    //    {
    //        return channelIds.SelectMany(x => GetSocketIdsByChannelId(x))
    //            .Distinct()
    //            .ToArray();
    //    }

    //    public string[] GetSocketIdsByChannelId(string channelId)
    //    {
    //        return _sockets.Values
    //            .Where(x => x.Subscriptions != null && x.Subscriptions.Contains(channelId))
    //            .Select(x => x.SocketId)
    //            .Distinct()
    //            .ToArray();
    //    }

    //    public string[] GetSocketIdsByIdentity(string identity)
    //    {
    //        List<string> ids;
    //        if (_socketIds.TryGetValue(identity, out ids))
    //            return ids.ToArray();
    //        return new string[0];
    //    }

    //    public SocketInfo[] GetSocketInfosByIdentity(string identity)
    //    {
    //        List<string> ids;
    //        if (_socketIds.TryGetValue(identity, out ids))
    //            return _sockets.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToArray();
    //        return new SocketInfo[0];
    //    }

    //    public SocketInfo[] GetSocketInfosByIdentities(IEnumerable<string> identities)
    //    {
    //        var ids = identities.Where(x => _socketIds.ContainsKey(x)).SelectMany(x => _socketIds[x]).Distinct().ToArray();
    //        if (ids != null && ids.Length > 0)
    //        {
    //            return _sockets.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToArray();
    //        }
    //        return new SocketInfo[0];
    //    }

    //    public string AddSocket(WebSocket socket, HttpContext context)
    //    {
    //        var connectionId = GetId(socket);
    //        if (connectionId != null)
    //        {
    //            return connectionId;
    //        }

    //        connectionId = CreateConnectionId(context);
    //        var socketInfo = new SocketInfo()
    //        {
    //            Socket = socket,
    //            ConnectionId = connectionId
    //        };
    //        context.Features.Set(new WebSocketFeature(socketInfo));

    //        _sockets.TryAdd(connectionId, socketInfo);
    //        Events?.InvokeOnSocketAdded(socketInfo, context);

    //        var identity = Options?.GetSocketIdentity?.Invoke(ServiceProvider, context);

    //        if (identity == null)
    //        {
    //            identity = context?.User?.Identity?.Name;
    //        }

    //        if (identity != null /*&& context.User.Identity.IsAuthenticated*/)
    //        {
    //            socketInfo.Identity = identity;
    //            socketInfo.Roles = ((ClaimsIdentity)context?.User?.Identity)?
    //                .Claims?
    //                .Where(c => c.Type == ClaimTypes.Role)?
    //                .Select(c => c.Value);

    //            List<string> connectionIds;
    //            if (_socketIds.TryGetValue(identity, out connectionIds))
    //            {
    //                if (!connectionIds.Contains(connectionId))
    //                {
    //                    connectionIds.Add(connectionId);
    //                }
    //            }
    //            else
    //            {
    //                connectionIds = new List<string>();
    //                connectionIds.Add(connectionId);
    //                _socketIds.TryAdd(identity, connectionIds);
    //            }
    //        }
    //        return connectionId;
    //    }

    //    public async Task RemoveSocketByIdentity(string identity, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
    //    {
    //        var ids = GetSocketIdsByIdentity(identity);
    //        var tasks = ids.Select(x => RemoveSocket(x, closeStatus));
    //        await Task.WhenAll(tasks);
    //    }

    //    public async Task RemoveSocket(string id, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
    //    {
    //        SocketInfo socketInfo;
    //        _sockets.TryRemove(id, out socketInfo);
    //        Events?.InvokeOnSocketRemoved(socketInfo);

    //        var containingSockets = _socketIds.Where(x => x.Value.Contains(id)).ToDictionary(x => x.Key, x => x.Value);
    //        foreach (var sock in containingSockets)
    //        {
    //            sock.Value.Remove(id);
    //        }

    //        var socksToRemove = containingSockets.Where(x => x.Value.Count == 0).Select(x => x.Key).ToArray();
    //        foreach (var sockToRemove in socksToRemove)
    //            _socketIds.TryRemove(sockToRemove, out List<string> s);

    //        if (socketInfo.Socket.State != WebSocketState.Closed)
    //            await socketInfo.Socket.CloseAsync(closeStatus: closeStatus,
    //                                    statusDescription: "Closed by the WebSocketManager",
    //                                    cancellationToken: CancellationToken.None);
    //    }

    //    public async Task<string> RemoveSocket(WebSocket socket, HttpContext context)
    //    {
    //        var id = GetId(socket);
    //        if (id == null)
    //        {
    //            return id;
    //        }

    //        SocketInfo socketInfo;
    //        _sockets.TryRemove(id, out socketInfo);
    //        Events?.InvokeOnSocketRemoved(socketInfo);

    //        if (context != null && context.User != null && context.User.Identity != null && context.User.Identity.Name != null)
    //        {
    //            List<string> connectionIds;
    //            if (_socketIds.TryGetValue(context.User.Identity.Name, out connectionIds))
    //            {
    //                if (!connectionIds.Contains(id))
    //                {
    //                    connectionIds.Remove(id);
    //                }

    //                if (connectionIds.Count == 0)
    //                {
    //                    _socketIds.TryRemove(context.User.Identity.Name, out connectionIds);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            var containingSockets = _socketIds.Where(x => x.Value.Contains(id)).ToDictionary(x => x.Key, x => x.Value);
    //            foreach (var sock in containingSockets)
    //            {
    //                sock.Value.Remove(id);
    //            }

    //            var socksToRemove = containingSockets.Where(x => x.Value.Count == 0).Select(x => x.Key).ToArray();
    //            foreach (var sockToRemove in socksToRemove)
    //                _socketIds.TryRemove(sockToRemove, out List<string> s);
    //        }

    //        try
    //        {
    //            await socketInfo.Socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
    //                                statusDescription: "Closed by the WebSocketManager",
    //                                cancellationToken: CancellationToken.None);
    //        }
    //        catch { }

    //        return id;
    //    }

    //    private string CreateConnectionId(HttpContext context)
    //    {
    //        if (context != null)
    //        {
    //            if (context.User != null)
    //            {
    //                if (context.User.Identity != null)
    //                {
    //                    if (context.User.Identity.IsAuthenticated)
    //                    {
    //                        if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
    //                        {
    //                            return $"{context.User.Identity.Name}-{Guid.NewGuid().ToString().Substring(0, 8)}";
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        return Guid.NewGuid().ToString();
    //    }
    //}
}

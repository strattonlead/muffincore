//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Security.Claims;
//using System.Security.Principal;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Muffin.Common.Util.WebSockets
//{
//    public class WebSocketsConnectionManager
//    {
//        private ConcurrentDictionary<string, SocketInfo> _sockets = new ConcurrentDictionary<string, SocketInfo>();
//        private ConcurrentDictionary<string, List<string>> _socketIds = new ConcurrentDictionary<string, List<string>>();

//        public WebSockets GetSocketById(string id)
//        {
//            var socketInfo = _sockets.FirstOrDefault(p => p.Key == id);
//            if (socketInfo.Value != null)
//                return socketInfo.Value.Socket;
//            return null;
//        }

//        public ConcurrentDictionary<string, WebSockets> GetAll()
//        {
//            return new ConcurrentDictionary<string, WebSockets>(_sockets.ToDictionary(x => x.Key, x => x.Value.Socket));
//        }

//        public SocketInfo[] GetAllSocketInfos()
//        {
//            return _sockets.Select(x => x.Value).ToArray();
//        }

//        public string GetPrincipalName(WebSockets socket)
//        {
//            var socketId = GetId(socket);
//            return _socketIds.Where(x => x.Value.Contains(socketId))
//                .Select(x => x.Key) // Hier sollte es tatsächlich nur einen geben...
//                .FirstOrDefault();
//        }

//        public string GetId(WebSockets socket)
//        {
//            var entry = _sockets.FirstOrDefault(p => p.Value.Socket == socket);
//            if (entry.Value != null)
//                return entry.Key;
//            return null;
//        }

//        public SocketInfo GetSocketInfoBySocket(WebSockets socket)
//        {
//            return _sockets.FirstOrDefault(p => p.Value.Socket == socket).Value;
//        }

//        public SocketInfo GetSocketInfoBySocketId(string socketId)
//        {
//            return _sockets.FirstOrDefault(p => string.Equals(p.Value.SocketId, socketId)).Value;
//        }

//        public string[] GetIds(IPrincipal principal)
//        {
//            return GetSocketIdsByIdentity(principal.Identity.Name);
//        }

//        public string[] GetSocketIdsByIdentity(string identity)
//        {
//            List<string> ids;
//            if (_socketIds.TryGetValue(identity, out ids))
//                return ids.ToArray();
//            return new string[0];
//        }

//        public SocketInfo[] GetSocketInfosByIdentity(string identity)
//        {
//            List<string> ids;
//            if (_socketIds.TryGetValue(identity, out ids))
//                return _sockets.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToArray();
//            return new SocketInfo[0];
//        }

//        public void AddSocket(WebSockets socket, HttpContext context)
//        {
//            var connectionId = GetId(socket);
//            if (connectionId != null)
//                return;

//            connectionId = CreateConnectionId();
//            var socketInfo = new SocketInfo()
//            {
//                Socket = socket,
//                SocketId = connectionId
//            };

//            _sockets.TryAdd(connectionId, socketInfo);

//            if (context != null && context.User != null && context.User.Identity != null && context.User.Identity.Name != null /*&& context.User.Identity.IsAuthenticated*/)
//            {
//                socketInfo.Identity = context.User.Identity.Name;
//                socketInfo.Roles = ((ClaimsIdentity)context.User.Identity)
//                    .Claims
//                    .Where(c => c.Type == ClaimTypes.Role)
//                    .Select(c => c.Value);

//                List<string> connectionIds;
//                if (_socketIds.TryGetValue(context.User.Identity.Name, out connectionIds))
//                {
//                    if (!connectionIds.Contains(connectionId))
//                    {
//                        connectionIds.Add(connectionId);
//                    }
//                }
//                else
//                {
//                    connectionIds = new List<string>();
//                    connectionIds.Add(connectionId);
//                    _socketIds.TryAdd(context.User.Identity.Name, connectionIds);
//                }
//            }
//        }

//        public async Task RemoveSocketByIdentity(string identity, WebSocketsCloseStatus closeStatus = WebSocketsCloseStatus.NormalClosure)
//        {
//            var ids = GetSocketIdsByIdentity(identity);
//            var tasks = ids.Select(x => RemoveSocket(x, closeStatus));
//            await Task.WhenAll(tasks);
//        }

//        public async Task RemoveSocket(string id, WebSocketsCloseStatus closeStatus = WebSocketsCloseStatus.NormalClosure)
//        {
//            SocketInfo socketInfo;
//            _sockets.TryRemove(id, out socketInfo);

//            var containingSockets = _socketIds.Where(x => x.Value.Contains(id)).ToDictionary(x => x.Key, x => x.Value);
//            foreach (var sock in containingSockets)
//            {
//                sock.Value.Remove(id);
//            }

//            var socksToRemove = containingSockets.Where(x => x.Value.Count == 0).Select(x => x.Key).ToArray();
//            foreach (var sockToRemove in socksToRemove)
//                _socketIds.TryRemove(sockToRemove, out List<string> s);

//            if (socketInfo.Socket.State != WebSocketstate.Closed)
//                await socketInfo.Socket.CloseAsync(closeStatus: closeStatus,
//                                        statusDescription: "Closed by the WebSocketsManager",
//                                        cancellationToken: CancellationToken.None);
//        }

//        public async Task RemoveSocket(string id, HttpContext context)
//        {
//            if (id == null)
//                return;

//            SocketInfo socketInfo;
//            _sockets.TryRemove(id, out socketInfo);

//            if (context != null && context.User != null && context.User.Identity != null && context.User.Identity.Name != null)
//            {
//                List<string> connectionIds;
//                if (_socketIds.TryGetValue(context.User.Identity.Name, out connectionIds))
//                {
//                    if (!connectionIds.Contains(id))
//                    {
//                        connectionIds.Remove(id);
//                    }

//                    if (connectionIds.Count == 0)
//                    {
//                        _socketIds.TryRemove(context.User.Identity.Name, out connectionIds);
//                    }
//                }
//            }
//            else
//            {
//                var containingSockets = _socketIds.Where(x => x.Value.Contains(id)).ToDictionary(x => x.Key, x => x.Value);
//                foreach (var sock in containingSockets)
//                {
//                    sock.Value.Remove(id);
//                }

//                var socksToRemove = containingSockets.Where(x => x.Value.Count == 0).Select(x => x.Key).ToArray();
//                foreach (var sockToRemove in socksToRemove)
//                    _socketIds.TryRemove(sockToRemove, out List<string> s);
//            }

//            await socketInfo.Socket.CloseAsync(closeStatus: WebSocketsCloseStatus.NormalClosure,
//                                    statusDescription: "Closed by the WebSocketsManager",
//                                    cancellationToken: CancellationToken.None);
//        }

//        private string CreateConnectionId()
//        {
//            return Guid.NewGuid().ToString();
//        }
//    }

//    public class SocketInfo
//    {
//        public string SocketId { get; set; }
//        public WebSockets Socket { get; set; }
//        public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;
//        public DateTime LastSendDate { get; set; } = DateTime.UtcNow;
//        public DateTime LastReceiveDate { get; set; } = DateTime.UtcNow;
//        public DateTime LastActionDate
//        {
//            get
//            {
//                return new DateTime[] {
//                    CreatedDateUtc,
//                    LastSendDate,
//                    LastReceiveDate
//                }.Max();
//            }
//        }
//        public string Identity { get; set; }
//        public IEnumerable<string> Roles { get; set; }

//        public List<string> Subscriptions { get; set; } = new List<string>();
//        public void Subscribe(string name)
//        {
//            if (string.IsNullOrWhiteSpace(name))
//            {
//                return;
//            }

//            if (Subscriptions.Contains(name))
//            {
//                return;
//            }

//            Subscriptions.Add(name);
//        }

//        public void Unsubscribe(string name)
//        {
//            if (string.IsNullOrWhiteSpace(name))
//            {
//                return;
//            }

//            if (Subscriptions.Contains(name))
//            {
//                Subscriptions.Remove(name);
//            }
//        }
//    }
//}

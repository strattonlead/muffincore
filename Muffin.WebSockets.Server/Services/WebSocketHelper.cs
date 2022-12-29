using Microsoft.Extensions.DependencyInjection;
using Muffin.Tenancy.Services.Abstraction;
using Muffin.WebSockets.Server.Handler;
using System;
using System.Linq;
using System.Net.WebSockets;

namespace Muffin.WebSockets.Server.Services
{
    public class WebSocketHelper
    {
        #region Properties

        public readonly IWebSocketContextAccessor WebSocketContextAccessor;
        public readonly WebSocketConnectionManager WebSocketConnectionManager;
        private readonly ITenantProvider TenantProvider;
        private long? _tenantId => TenantProvider?.ActiveTenant?.Id;

        public long[] ConnectedCredentials => SocketIds
            .Where(x => x != null)
            .Where(x => x.Split('-').Length >= 2)
            .Select(x => x.Split('-')[0])
            .Where(x => long.TryParse(x, out _))
            .Select(x => long.Parse(x))
            .Distinct()
            .ToArray();

        public string[] SocketIds => WebSocketConnectionManager.Connections.GetSocketInfos().Select(x => x.SocketId).ToArray();
        public string[] SocketIdsWithoutCurrent => SocketIds.Except(new string[] { CurrentWebSocketConnectionId }).ToArray();

        public long[] ConnectedCredentialsWithTenancy => SocketIdsWithTenancy
            .Where(x => x != null)
            .Where(x => x.Split('-').Length >= 2)
            .Select(x => x.Split('-')[0])
            .Where(x => long.TryParse(x, out _))
            .Select(x => long.Parse(x))
            .Distinct()
            .ToArray();

        public string[] SocketIdsWithTenancy => WebSocketConnectionManager.Connections.GetSocketInfos(_tenantId).SelectMany(x => x.Value).Select(x => x.Value.SocketId).ToArray();
        public string[] SocketIdsWithoutCurrentWithTenancy => SocketIds.Except(new string[] { CurrentWebSocketConnectionId }).ToArray();



        public WebSocketContext WebSocketContext => WebSocketContextAccessor.WebSocketContext.Value;
        public WebSocket WebSocket => WebSocketContext?.WebSocket;
        public SocketInfo WebSocketInfo => WebSocketContext?.WebSocketInfo;
        public string CurrentWebSocketConnectionId => WebSocketInfo?.SocketId;
        public long? CurrentCredentialId => long.TryParse(CurrentWebSocketConnectionId, out _) ? long.Parse(CurrentWebSocketConnectionId) : null;

        #endregion

        #region Constructor

        public WebSocketHelper(IServiceProvider serviceProvider)
        {
            WebSocketContextAccessor = serviceProvider.GetRequiredService<IWebSocketContextAccessor>();
            WebSocketConnectionManager = serviceProvider.GetRequiredService<WebSocketConnectionManager>();
            TenantProvider = serviceProvider.GetService<ITenantProvider>();
        }

        #endregion
    }

    public static class WebSocketHelperExtensions
    {
        public static void AddDefaultWebSocketHelper(this IServiceCollection services)
        {
            services.AddScoped<WebSocketHelper>();
        }
    }
}

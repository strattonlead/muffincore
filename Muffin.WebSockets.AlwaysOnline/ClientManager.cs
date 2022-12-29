using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.WebSockets.AlwaysOnline
{
    public abstract class ClientManager : BackgroundService { }
    public abstract class ClientManager<TKey, TClient, TAppState> : ClientManager
    {
        #region Properties

        protected readonly IServiceScopeFactory ServiceScopeFactory;
        protected Dictionary<TKey, TClient> Clients { get; private set; }

        #endregion

        #region Constructor

        public ClientManager(IServiceProvider serviceProvider)
        {
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        #endregion

        #region ClientManager

        public TClient GetClient(TKey key)
        {
            if (Clients != null)
            {
                if (Clients.TryGetValue(key, out TClient client))
                {
                    return client;
                }

                client = LoadClient(key);
                if (client != null)
                {
                    Clients[key] = client;
                    return client;
                }
            }

            return default;
        }

        public TClient[] GetClients(IEnumerable<TKey> keys)
        {
            if (Clients != null)
            {
                return keys.Where(x => Clients.ContainsKey(x))
                           .Select(x => Clients[x])
                           .ToArray();
            }
            return null;
        }

        public abstract TClient Authenticate(TKey key, string password);

        public abstract Dictionary<TKey, TClient> LoadClients();

        public abstract TClient LoadClient(TKey key);

        public abstract void AddClient(TKey key, TClient client);

        #endregion

        #region IHostedService

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested && Clients == null)
                {
                    try
                    {
                        Clients = LoadClients();
                        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                    }
                    catch (Exception) { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
                }
            }, stoppingToken);
        }

        #endregion
    }

    public static class ClientManagerExtensions
    {
        public static void AddClientManager<TImplementation>(this IServiceCollection services)
            where TImplementation : ClientManager, IHostedService
        {
            services.AddSingleton<TImplementation>();
            services.AddHostedService(s => s.GetService<TImplementation>());
        }
    }
}

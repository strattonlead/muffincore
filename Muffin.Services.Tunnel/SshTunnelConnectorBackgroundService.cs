using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Muffin.BackgroundServices;
using Muffin.Common.Util;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net;
using System.Text;

namespace Muffin.Services.Tunnel
{
    public class SshTunnelConnectorBackgroundService<TService> : ScheduledBackgroundService
            where TService : ISshTunnelOptionsProvider
    {
        #region Properties

        private SshClient? Client;
        private ForwardedPortRemote? ForwardedPort;
        private readonly SshTunnelEvents<TService> Events;
        private readonly SShTunnelController<TService> Controller;

        #endregion

        #region Constructors

        public SshTunnelConnectorBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Events = serviceProvider.GetRequiredService<SshTunnelEvents<TService>>();
            Controller = serviceProvider.GetRequiredService<SShTunnelController<TService>>();
            Controller.OnDisconnect += Controller_OnDisconnect;
        }



        #endregion

        #region SSH Controller

        private void Controller_OnDisconnect(object sender, SshTunnelControllerEventArgs e)
        {
            _stopTunneling();
        }

        private void _stopTunneling()
        {
            if (ForwardedPort != null && ForwardedPort.IsStarted)
            {
                ForwardedPort.Stop();
                ForwardedPort = null;
            }

            if (Client != null && Client.IsConnected)
            {
                Client.Disconnect();
                Client = null;
            }
        }

        #endregion

        #region IHostedService

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _stopTunneling();
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteScopedAsync(IServiceProvider scope, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (ForwardedPort != null && ForwardedPort.IsStarted)
                {
                    ForwardedPort.Stop();
                }

                if (Client != null && Client.IsConnected)
                {
                    Client.Disconnect();
                }
                return;
            }

            if (Client != null && Client.IsConnected)
            {
                return;
            }

            var optionsProvider = scope.GetService<ISshTunnelOptionsProvider>();
            if (optionsProvider == null)
            {
                await StopAsync(cancellationToken);
                return;
            }

            var getOptions = await optionsProvider.GetOptions(scope, cancellationToken);
            if (getOptions == null)
            {
                return;
            }

            if (getOptions.StopService)
            {
                await StopAsync(cancellationToken);
                return;
            }

            var options = getOptions.Options;

            if (!options.LocalPort.HasValue)
            {
                options.LocalPort = 5000;

                var server = scope.GetService<IServer>();
                if (server == null)
                {
                    await StopAsync(cancellationToken);
                    return;
                }

                var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
                if (serverAddressesFeature == null)
                {
                    await StopAsync(cancellationToken);
                    return;
                }

                var localBindingAddress = serverAddressesFeature.Addresses?.FirstOrDefault(x => !x.StartsWith("https"));
                if (string.IsNullOrWhiteSpace(localBindingAddress))
                {
                    return;
                }

                if (localBindingAddress.Split(':').Length > 1)
                {
                    var temp = localBindingAddress.Split(':').Last()?.Replace("/", "");
                    if (uint.TryParse(temp, out var tempPort))
                    {
                        options.LocalPort = tempPort;
                    }
                }
            }

            PrivateKeyFile? privateKeyFile = null;
            if (!string.IsNullOrWhiteSpace(options.PrivateKeyFilePath))
            {
                privateKeyFile = new PrivateKeyFile(options.PrivateKeyFilePath);
            }
            else if (!string.IsNullOrWhiteSpace(options.PrivateKeyString))
            {
                var privateKeyBytes = Encoding.UTF8.GetBytes(options.PrivateKeyString);
                using (var privateKeyStream = privateKeyBytes.ToStream())
                {
                    privateKeyFile = new PrivateKeyFile(privateKeyStream);
                }
            }

            if (Client == null)
            {
                Client = new SshClient(options.Host, options.Port, options.Username, privateKeyFile);
                Client.ErrorOccurred += delegate (object sender, ExceptionEventArgs e)
                {
                    Events?.InvokeClientException(sender, e);
                };
            }

            if (Client.IsConnected)
            {
                return;
            }

            Client.Connect();
            //scope.Logger.LogInformation("SSH Client connected");

            if (ForwardedPort != null)
            {
                ForwardedPort.Stop();
            }

            var addr = IPAddress.Parse("127.0.0.1");
            ForwardedPort = new ForwardedPortRemote(addr, options.RemotePort, addr, options.LocalPort.Value);
            ForwardedPort.Exception += delegate (object sender, ExceptionEventArgs e)
            {
                Events?.InvokeForwardedPortException(sender, e);
            };
            Client.AddForwardedPort(ForwardedPort);
            ForwardedPort.Start();
        }

        #endregion
    }

    public class SShTunnelController<TService>
            where TService : ISshTunnelOptionsProvider
    {
        public void Disconnect()
        {
            OnDisconnect?.Invoke(this, new SshTunnelControllerEventArgs());
        }

        internal event SshTunnelControllerEvent OnDisconnect;
    }

    public class SshTunnelControllerEventArgs { }

    public delegate void SshTunnelControllerEvent(object sender, SshTunnelControllerEventArgs e);

    public class SshTunnelEvents<TService>
            where TService : ISshTunnelOptionsProvider
    {
        public EventHandler<ExceptionEventArgs> ClientException;
        public EventHandler<ExceptionEventArgs> ForwardedPortException;

        internal void InvokeClientException(object sender, ExceptionEventArgs e)
        {
            ClientException?.Invoke(sender, e);
        }

        internal void InvokeForwardedPortException(object sender, ExceptionEventArgs e)
        {
            ForwardedPortException?.Invoke(sender, e);
        }
    }

    public static class SshTunnelConnectorBackgroundServiceExtensions
    {
        public static void AddSshTunnelConnectorBackgroundService<SshTunnelOptionsProvider>(this IServiceCollection services)
            where SshTunnelOptionsProvider : class, ISshTunnelOptionsProvider
        {
            services.AddSshTunnelConnectorBackgroundService<SshTunnelOptionsProvider>(TimeSpan.FromSeconds(3));
        }

        public static void AddSshTunnelConnectorBackgroundService<SshTunnelOptionsProvider>(this IServiceCollection services, TimeSpan interval)
            where SshTunnelOptionsProvider : class, ISshTunnelOptionsProvider
        {
            services.AddSingleton<ISshTunnelOptionsProvider, SshTunnelOptionsProvider>();
            services.AddSingleton<SshTunnelEvents<SshTunnelOptionsProvider>>();
            services.AddSingleton<SShTunnelController<SshTunnelOptionsProvider>>();
            services.AddScoped<SshTunnelOptionsProvider>();
            services.AddScheduledBackgroundService<SshTunnelConnectorBackgroundService<SshTunnelOptionsProvider>>(options => options.UseInterval(interval));
        }
    }
}
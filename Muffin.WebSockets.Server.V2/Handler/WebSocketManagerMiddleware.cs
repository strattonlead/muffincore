using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.WebSockets.Server.Security;
using Muffin.WebSockets.Server.Services;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Handler
{
    public class WebSocketManagerMiddleware<T>
        where T : class, IWebSocketAuthorizationHandler, new()
    {
        private readonly RequestDelegate _next;
        private WebSocketConnectionHandler _WebSocketHandler { get; set; }
        protected T AuthorizationHandler { get; set; }

        protected readonly IServiceProvider ServiceProvider;
        private readonly ILogger Logger;

        public WebSocketManagerMiddleware(RequestDelegate next, WebSocketConnectionHandler WebSocketHandler, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            _next = next;
            _WebSocketHandler = WebSocketHandler;
            AuthorizationHandler = Activator.CreateInstance<T>();
            Logger = serviceProvider.GetService<ILogger<WebSocketManagerMiddleware<T>>>();
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            if (AuthorizationHandler != null)
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    if (!AuthorizationHandler.Authorize(context, scope.ServiceProvider))
                    {
                        return;
                    }
                }
            }

            try
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var id = _WebSocketHandler.OnConnected(socket, context);

                // Der Socket wurde doch nicht angenommen bzw gleich wieder getrennt
                if (id == null)
                {
                    Logger?.LogWarning("WebSocket was not accepted.");
                    var protocols = context.WebSockets.WebSocketRequestedProtocols?.ToArray();
                    if (protocols != null)
                    {
                        Logger?.LogWarning($"Requested Protocols: {string.Join(", ", protocols)}");
                    }
                    return;
                }
                else
                {
                    Logger?.LogInformation($"WebSocket connected -> {id}");
                }

                try
                {
                    await Receive(socket, context, async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            await _WebSocketHandler.ReceiveAsync(socket, result, buffer, context);
                            return;
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Logger?.LogInformation($"WebSocket close -> {id}");
                            await _WebSocketHandler.OnDisconnected(socket, context);
                            return;
                        }

                        //if (socket.State != WebSockettate.Closed)
                        //{
                        //    await _WebSocketHandler.OnDisconnected(socket, context);
                        //}
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await _WebSocketHandler.OnError(socket, context);
                    await _WebSocketHandler.OnDisconnected(socket, context);
                }
            }
            catch (Exception e2)
            {
                Logger?.LogError($"AcceptWebSocketAsync possible error: {e2.ToString()}");
            }
        }

        private async Task Receive(WebSocket socket, HttpContext context, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];


            WebSocketReceiveResult result = null;

            while (socket.State == WebSocketState.Open)
            {
                using (var mem = new MemoryStream())
                {
                    do
                    {
                        var segment = new ArraySegment<byte>(buffer);
                        result = await socket.ReceiveAsync(buffer: segment,
                                                              cancellationToken: CancellationToken.None);

                        for (var i = 0; i < result.Count; i++)
                        {
                            mem.WriteByte(segment.Array[i]);
                        }
                    } while (!result.EndOfMessage);

                    mem.Seek(0, SeekOrigin.Begin);
                    handleMessage(result, mem.ToArray());
                    mem.Flush();
                }
            }

            if (socket.State == WebSocketState.Closed)
            {
                try { await _WebSocketHandler.OnDisconnected(socket, context); } catch { }
            }

            if (socket.State == WebSocketState.Aborted)
            {
                try { await _WebSocketHandler.OnAborted(socket, context); } catch { }
            }
        }
    }

    public static class WebSocketManagerMiddlewareExtensions
    {
        public static IApplicationBuilder MapWebSocketManager<T>(this IApplicationBuilder app,
                                                              PathString path,
                                                              WebSocketConnectionHandler handler)
            where T : class, IWebSocketAuthorizationHandler, new()
        {
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketManagerMiddleware<T>>(handler));
        }

        public static IApplicationBuilder MapWebSocketManager<T>(this IApplicationBuilder app,
                                                              PathString path,
                                                              params object[] args)
            where T : class, IWebSocketAuthorizationHandler, new()
        {
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketManagerMiddleware<T>>(args));
        }

        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
        {
            services.AddWebSocketManager(Assembly.GetEntryAssembly());
            return services;
        }

        public static IServiceCollection AddWebSocketManager(this IServiceCollection services, Assembly assembly)
        {
            services.AddWebSocketManager(assembly, null);
            return services;
        }

        private static bool AddedManager = false;
        private static bool AddedEvents = false;
        public static IServiceCollection AddWebSocketManager(this IServiceCollection services, Assembly assembly, Action<WebSocketManagerOptionsBuilder> options)
        {
            if (options != null)
            {
                var optionsBuilder = new WebSocketManagerOptionsBuilder();
                options.Invoke(optionsBuilder);

                var socketOptions = optionsBuilder.Build();
                services.AddSingleton(socketOptions);
            }

            if (!AddedManager)
            {
                services.AddSingleton<WebSocketConnectionManager>();
                services.AddWebSocketSubscriptionService();
                services.AddWebSocketContextAccessor();
                services.AddDefaultWebSocketHelper();
                AddedManager = true;
            }

            foreach (var type in assembly.ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketConnectionHandler))
                {
                    services.AddSingleton(type);
                }
            }

            if (!AddedEvents)
            {
                AddedEvents = true;
                services.AddSingleton<WebSocketHandlerEvents>();
            }

            return services;
        }

        public static IServiceCollection AddWebSocketManager<T>(this IServiceCollection services)
        {
            services.AddWebSocketManager<T>(Assembly.GetEntryAssembly());
            return services;
        }

        public static IServiceCollection AddWebSocketManager<T>(this IServiceCollection services, Assembly assembly)
        {
            services.AddWebSocketManager<T>(assembly, null);
            return services;
        }

        public static IServiceCollection AddWebSocketManager<T>(this IServiceCollection services, Assembly assembly, Action<WebSocketManagerOptionsBuilder> options)
        {
            if (options != null)
            {
                var optionsBuilder = new WebSocketManagerOptionsBuilder();
                options.Invoke(optionsBuilder);

                var socketOptions = optionsBuilder.Build();
                services.AddSingleton(socketOptions);
            }

            if (!AddedManager)
            {
                services.AddSingleton<WebSocketConnectionManager>();
                services.AddWebSocketSubscriptionService();
                services.AddWebSocketContextAccessor();
                services.AddDefaultWebSocketHelper();
                AddedManager = true;
            }

            foreach (var type in assembly.ExportedTypes)
            {
                if (typeof(T).IsAssignableFrom(type))
                {
                    services.AddSingleton(type);
                }
            }

            if (!AddedEvents)
            {
                AddedEvents = true;
                services.AddSingleton<WebSocketHandlerEvents>();
            }

            return services;
        }
    }

    public class WebSocketManagerOptionsBuilder
    {
        private WebSocketManagerOptions Options = new WebSocketManagerOptions();
        public WebSocketManagerOptionsBuilder UseSocketIdentity(Func<IServiceProvider, HttpContext, string> func)
        {
            Options.GetSocketIdentity = func;
            return this;
        }

        public WebSocketManagerOptions Build()
        {
            return Options;
        }
    }

    public class WebSocketManagerOptions
    {
        public Func<IServiceProvider, HttpContext, string> GetSocketIdentity { get; set; }
    }
}

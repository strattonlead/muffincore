//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.IO;
//using System.Net.WebSockets;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Muffin.Common.Util.WebSockets
//{
//    public class WebSocketsManagerMiddleware<T>
//        where T : class, IWebSocketsAuthorizationHandler, new()
//    {
//        private readonly RequestDelegate _next;
//        private WebSocketsHandler _WebSocketsHandler { get; set; }
//        protected T AuthorizationHandler { get; set; }

//        protected readonly IServiceProvider ServiceProvider;

//        public WebSocketsManagerMiddleware(RequestDelegate next, WebSocketsHandler WebSocketsHandler, IServiceProvider serviceProvider)
//        {
//            ServiceProvider = serviceProvider;
//            _next = next;
//            _WebSocketsHandler = WebSocketsHandler;
//            AuthorizationHandler = Activator.CreateInstance<T>();
//        }

//        public async Task Invoke(HttpContext context)
//        {
//            if (!context.WebSockets.IsWebSocketsRequest)
//                return;

//            if (AuthorizationHandler != null)
//            {
//                if (!AuthorizationHandler.Authorize(context, ServiceProvider))
//                    return;
//            }

//            var socket = await context.WebSockets.AcceptWebSocketsAsync();
//            _WebSocketsHandler.OnConnected(socket, context);

//            try
//            {
//                await Receive(socket, context, async (result, buffer) =>
//                           {
//                               if (result.MessageType == WebSocketsMessageType.Text)
//                               {
//                                   await _WebSocketsHandler.ReceiveAsync(socket, result, buffer, context);
//                                   return;
//                               }

//                               else if (result.MessageType == WebSocketsMessageType.Close)
//                               {
//                                   await _WebSocketsHandler.OnDisconnected(socket, context);
//                                   return;
//                               }

//                           });
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                await _WebSocketsHandler.OnError(socket, context);
//                await _WebSocketsHandler.OnDisconnected(socket, context);
//            }

//            //TODO - investigate the Kestrel exception thrown when this is the last middleware
//            //await _next?.Invoke(context);
//        }

//        private async Task Receive(WebSockets socket, HttpContext context, Action<WebSocketsReceiveResult, byte[]> handleMessage)
//        {
//            var buffer = new byte[1024 * 4];


//            WebSocketsReceiveResult result = null;

//            while (socket.State == WebSocketstate.Open)
//            {
//                using (var mem = new MemoryStream())
//                {
//                    do
//                    {
//                        var segment = new ArraySegment<byte>(buffer);
//                        result = await socket.ReceiveAsync(buffer: segment,
//                                                              cancellationToken: CancellationToken.None);

//                        for (var i = 0; i < result.Count; i++)
//                        {
//                            mem.WriteByte(segment.Array[i]);
//                        }
//                    } while (!result.EndOfMessage);

//                    mem.Seek(0, SeekOrigin.Begin);
//                    handleMessage(result, mem.ToArray());
//                    mem.Flush();
//                }
//            }


//            if (socket.State == WebSocketstate.Aborted)
//                try { await _WebSocketsHandler.OnAborted(socket, context); } catch { }
//        }
//    }

//    public static class WebSocketsManagerMiddlewareExtensions
//    {
//        public static IApplicationBuilder MapWebSocketsManager<T>(this IApplicationBuilder app,
//                                                              PathString path,
//                                                              WebSocketsHandler handler)
//            where T : class, IWebSocketsAuthorizationHandler, new()
//        {
//            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketsManagerMiddleware<T>>(handler));
//        }

//        public static IApplicationBuilder MapWebSocketsManager<T>(this IApplicationBuilder app,
//                                                              PathString path,
//                                                              params object[] args)
//            where T : class, IWebSocketsAuthorizationHandler, new()
//        {
//            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketsManagerMiddleware<T>>(args));
//        }

//        public static IServiceCollection AddWebSocketsManager(this IServiceCollection services)
//        {
//            services.AddWebSocketsManager(Assembly.GetEntryAssembly());
//            return services;
//        }

//        public static IServiceCollection AddWebSocketsManager(this IServiceCollection services, Assembly assembly)
//        {
//            services.AddTransient<WebSocketsConnectionManager>();

//            foreach (var type in assembly.ExportedTypes)
//            {
//                if (type.GetTypeInfo().BaseType == typeof(WebSocketsHandler))
//                {
//                    services.AddSingleton(type);
//                }
//            }
//            return services;
//        }

//        public static IServiceCollection AddWebSocketsManager<T>(this IServiceCollection services)
//        {
//            services.AddWebSocketsManager<T>(Assembly.GetEntryAssembly());
//            return services;
//        }

//        public static IServiceCollection AddWebSocketsManager<T>(this IServiceCollection services, Assembly assembly)
//        {
//            services.AddTransient<WebSocketsConnectionManager>();

//            foreach (var type in assembly.ExportedTypes)
//            {
//                if (typeof(T).IsAssignableFrom(type))
//                {
//                    services.AddSingleton(type);
//                }
//            }

//            return services;
//        }
//    }
//}

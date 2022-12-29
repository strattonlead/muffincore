using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muffin.Common.Api.WebSockets;
using Muffin.Common.Util;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Muffin.WebSockets.Client
{
    public abstract class WebSocketClient
    {
        public WebSocket WebSocket { get; protected set; }
    }

    public class WebSocketClient<T> : WebSocketClient
        where T : WebSocketAuthorizer
    {
        #region Properties

        private readonly WebSocketClientOptions Options;
        private readonly WebSocketCookieProvider<T> WebSocketCookieProvider;
        private readonly WebSocketClientEvents<T> WebSocketEvents;
        private readonly WebSocketApiEvents ApiEvents;
        private readonly T WebSocketAuthorizer;
        private readonly IHostApplicationLifetime HostApplicationLifetime;
        private readonly ILogger<WebSocketClient<T>> Logger;

        #endregion

        #region Constructor

        public WebSocketClient(IServiceProvider serviceProvider)
        {
            Options = serviceProvider.GetRequiredService<WebSocketClientOptions>();
            WebSocketCookieProvider = serviceProvider.GetRequiredService<WebSocketCookieProvider<T>>();
            WebSocketEvents = serviceProvider.GetRequiredService<WebSocketClientEvents<T>>();

            if (Options.ApiEventsType != null)
            {
                ApiEvents = (WebSocketApiEvents)serviceProvider.GetRequiredService(Options.ApiEventsType);
            }

            WebSocketAuthorizer = serviceProvider.GetRequiredService<T>();
            HostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            Logger = serviceProvider.GetService<ILogger<WebSocketClient<T>>>();

            HostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                Close();
            });
        }

        #endregion

        #region Control

        public async Task<WebSocket> Connect()
        {
            Logger?.LogInformation($"{nameof(WebSocketClient<T>)} {nameof(Connect)}");
            var result = await _connect();
            StartKeepAliveTask();
            return result;
        }

        private async Task<WebSocket> _connect()
        {
            if (WebSocket != null)
            {
                return WebSocket;
            }

            Logger?.LogInformation($"WebSocketAuthorizer? {WebSocketAuthorizer != null}");
            if (WebSocketAuthorizer != null)
            {
                var success = await WebSocketAuthorizer.Authorize();
                if (!success)
                {
                    Logger?.LogInformation($"WebSocketAuthorizer login failed!");
                    return null;
                }
            }

            Logger?.LogInformation($"Create WebSocket with url: {Options.Url}");
            WebSocket = new WebSocket(Options.Url);

            Logger?.LogInformation($"UseProxy? {Options.UseProxy}");
            if (Options.UseProxy)
            {
                Logger?.LogInformation($"ProxyParams: {Options.ProxyUrl} Username: {Options.ProxyUsername} Password: {!string.IsNullOrWhiteSpace(Options.ProxyPassword)}");
                WebSocket.SetProxy(Options.ProxyUrl, Options.ProxyUsername, Options.ProxyPassword);
            }

            Logger?.LogInformation($"WebSocketCookieProvider? {WebSocketCookieProvider != null}");
            var cookies = WebSocketCookieProvider?.GetCookies();
            Logger?.LogInformation($"Cookies? {cookies?.Length}");
            if (cookies != null)
            {
                foreach (var cookie in cookies)
                {
                    WebSocket.SetCookie(cookie);
                }
            }

            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnOpen += WebSocket_OnOpen;
            WebSocket.OnClose += WebSocket_OnClose;

            Logger?.LogInformation($"Connect...");
            WebSocket.Connect();
            Logger?.LogInformation($"Connected!");
            return WebSocket;
        }

        public void Close()
        {
            Logger?.LogInformation($"{nameof(WebSocketClient<T>)} {nameof(Close)}");
            if (WebSocket == null)
            {
                Logger?.LogInformation($"Nothing to close.");
                return;
            }


            WebSocket.Close();
            WebSocket.OnMessage -= WebSocket_OnMessage;
            WebSocket.OnError -= WebSocket_OnError;
            WebSocket.OnOpen -= WebSocket_OnOpen;
            WebSocket.OnClose -= WebSocket_OnClose;
            WebSocket = null;
            Logger?.LogInformation($"Closed.");

        }

        #endregion

        #region Keep Alive

        public bool IsAlive
        {
            get
            {
                return WebSocket?.IsAlive ?? false;
            }
        }

        private Task KeepAliveTask;
        private void StartKeepAliveTask()
        {
            Logger?.LogInformation($"{nameof(StartKeepAliveTask)} KeepAlive? {Options.KeepAlive} KeepAliveTask? {KeepAliveTask != null}");
            if (!Options.KeepAlive || KeepAliveTask != null)
            {
                Logger?.LogInformation($"Skip {nameof(StartKeepAliveTask)}");
                return;
            }

            KeepAliveTask = Task.Run(async () =>
            {
                Logger?.LogInformation($"Run KeepAliveTask");
                while (!HostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    try
                    {
                        if (!WebSocket?.IsAlive ?? true)
                        {
                            Logger?.LogInformation($"Socket is not alive... Reconnect...");
                            Close();
                            await _connect();
                            Logger?.LogInformation($"Socket reconnected!");
                        }

                        //if (!WebSocket?.IsAlive ?? true)
                        //{

                        //}
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e.Message);
                        Logger.LogTrace(e.ToString());
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                    catch { }
                }
                Logger?.LogInformation($"Finished KeepAliveTask!");
            }, HostApplicationLifetime.ApplicationStopping);
        }

        #endregion

        #region Events

        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler OnOpen;

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            WebSocketEvents?.InvokeOnClose(sender, e);
            OnClose?.Invoke(sender, e);
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            WebSocketEvents?.InvokeOnOpen(sender, e);
            OnOpen?.Invoke(sender, e);
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            WebSocketEvents?.InvokeOnError(sender, e);
            OnError?.Invoke(sender, e);
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            WebSocketEvents?.InvokeOnMessage(sender, e);
            OnMessage?.Invoke(sender, e);
            HandleMessageForApiRequest(sender, e);
        }

        #endregion

        #region Receive

        protected virtual MulticastDelegate GetEventDelegate(FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(this) as MulticastDelegate;
        }

        protected virtual void HandleMessageForApiRequest(object sender, MessageEventArgs e)
        {
            if (e.IsPing)
                return;

            var request = JsonConvert.DeserializeObject<ApiRequest>(e.Data);
            ApiEvents?.Invoke(request);
        }

        #endregion
    }

    public class WebSocketClientOptions
    {
        public Type ApiEventsType { get; set; }
        public bool LoginAfterAuthorize { get; set; }
        public string Url { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyUrl { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public bool KeepAlive { get; set; }
    }

    public class WebSocketClientOptionsBuilder
    {
        private WebSocketClientOptions Options = new WebSocketClientOptions();

        public WebSocketClientOptionsBuilder UseApiEvents<T>()
            where T : WebSocketApiEvents
        {
            return UseApiEvents(typeof(T));
        }

        public WebSocketClientOptionsBuilder UseApiEvents(Type type)
        {
            Options.ApiEventsType = type;
            return this;
        }

        public WebSocketClientOptionsBuilder LoginAfterAuthorize(bool doAuth)
        {
            Options.LoginAfterAuthorize = doAuth;
            return this;
        }

        public WebSocketClientOptionsBuilder UseUrl(string url)
        {
            Options.Url = url;
            return this;
        }

        public WebSocketClientOptionsBuilder KeepAlive()
        {
            Options.KeepAlive = true;
            return this;
        }

        public WebSocketClientOptionsBuilder Use()
        {
            Options.KeepAlive = true;
            return this;
        }

        public WebSocketClientOptionsBuilder UseProxy(string url, int port, string username, string password)
        {
            Options.UseProxy = true;
            if (port != 80)
            {
                Options.ProxyUrl = $"{url}:{port}";
            }
            else
            {
                Options.ProxyUrl = url;
            }
            Options.ProxyUsername = username;
            Options.ProxyPassword = password;
            return this;
        }

        internal WebSocketClientOptions Build()
        {
            return Options;
        }
    }

    public class WebSocketClientEvents<T>
        where T : WebSocketAuthorizer
    {
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler OnOpen;

        internal void InvokeOnClose(object sender, CloseEventArgs e)
        {
            OnClose?.Invoke(sender, e);
        }

        internal void InvokeOnOpen(object sender, EventArgs e)
        {
            OnOpen?.Invoke(sender, e);
        }

        internal void InvokeOnError(object sender, ErrorEventArgs e)
        {
            OnError?.Invoke(sender, e);
        }

        internal void InvokeOnMessage(object sender, MessageEventArgs e)
        {
            OnMessage?.Invoke(sender, e);
        }
    }

    public abstract class WebSocketApiEvents
    {
        protected virtual MulticastDelegate GetEventDelegate(FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(this) as MulticastDelegate;
        }

        internal void Invoke(ApiRequest request)
        {
            try
            {
                var fieldInfo = PropertyHelper
                                .GetFieldInfosIncludingBaseClasses(GetType(), BindingFlags.Instance | BindingFlags.NonPublic)
                                .FirstOrDefault(x => string.Equals(x.Name, "On" + request.Action));

                if (fieldInfo != null)
                {
                    var parameters = request.Params.Select(x => x.Value).Cast<object>().ToArray();
                    var eventDelegate = GetEventDelegate(fieldInfo);
                    if (eventDelegate != null)
                    {
                        foreach (var handler in eventDelegate.GetInvocationList())
                        {
                            var methodParameters = handler.Method.GetParameters();
                            var typeSafeParameters = new object[methodParameters.Length];
                            for (var i = 0; i < methodParameters.Length; i++)
                            {
                                var methodParameter = methodParameters[i];
                                var rawValue = parameters[i];
                                var jsonValue = JsonConvert.SerializeObject(rawValue);
                                var typeSafeValue = JsonConvert.DeserializeObject(jsonValue, methodParameter.ParameterType/*, DeserializableInterfaceJsonHelper.GetInterfaceJsonSerializerSettings()*/);
                                typeSafeParameters[i] = typeSafeValue;
                            }
                            handler.Method.Invoke(handler.Target, typeSafeParameters);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
    }

    public static class WebSocketClientExtensions
    {
        public static void AddWebSocketClient<T>(this IServiceCollection services, Action<WebSocketClientOptionsBuilder> action)
            where T : WebSocketAuthorizer
        {
            if (action != null)
            {
                var optionsBuilder = new WebSocketClientOptionsBuilder();
                action(optionsBuilder);
                var options = optionsBuilder.Build();
                services.AddSingleton(options);
            }
            services.AddSingleton<WebSocketClientEvents<T>>();
            services.AddSingleton<WebSocketClient<T>>();
        }
    }
}

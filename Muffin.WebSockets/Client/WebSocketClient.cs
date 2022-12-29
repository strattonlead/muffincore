using Muffin.Common.Api.WebSockets;
using Muffin.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using WebSocketSharp;

namespace Muffin.WebSockets
{
    public interface IWebSocketClient
    {
        void Connect();
        void Disconnect();
    }

    public class WebSocketClient : IDisposable, IWebSocketClient
    {
        #region Properties

        protected WebSocket _socket;
        private Timer _timer;
        protected string Url { get; private set; }
        protected bool LockApiRequests { get; set; }
        protected MessageBus MessageBus { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsShouldDisconnect { get; set; }
        public bool IsShutdownReceived { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool PreAuth { get; set; }
        private List<WebSocketClientReceiver> ClientReceivers { get; set; } = new List<WebSocketClientReceiver>();

        #endregion

        public WebSocketClient(string url)
        {
            _init(url, null, null, false);
        }

        public WebSocketClient(string url, string username, string password, bool preAuth)
        {
            _init(url, username, password, preAuth);
        }

        public const string WEB_SOCKET_CLIENT_MESSAGE_BUS = nameof(WEB_SOCKET_CLIENT_MESSAGE_BUS);
        private void _init(string url, string username, string password, bool preAuth)
        {
            MessageBus = MessageBus.GetInstance($"{WEB_SOCKET_CLIENT_MESSAGE_BUS}_{Guid.NewGuid().ToString().Substring(0, 8)}");
            Url = url;
            Username = username;
            Password = password;
            PreAuth = preAuth;

        }

        public virtual void Connect()
        {
            if (!IsConnected)
            {
                IsShouldDisconnect = false;

                if (_socket != null)
                {
                    _socket.OnOpen -= _onOpen;
                    _socket.OnError -= _onError;
                    _socket.OnClose -= _onClose;
                    _socket.OnMessage -= _onMessage;
                    _socket = null;
                }

                _socket = new WebSocket(Url);

                if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password))
                {
                    _socket.SetCredentials(Username, Password, PreAuth);
                }

                _socket.OnOpen += _onOpen;
                _socket.OnError += _onError;
                _socket.OnClose += _onClose;
                _socket.OnMessage += _onMessage;

                OnBeforeConnect?.Invoke(this, _socket);

                _socket.Connect();
            }

        }

        public void Shutdown()
        {
            IsShutdownReceived = true;
        }

        public virtual void Disconnect()
        {
            IsShouldDisconnect = true;
            if (_socket != null)
            {
                _socket.Close();
                OnDisconnected?.Invoke(this);
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Ping();
                    //_reconnect();
                }
            }
            catch { }
        }

        private bool IsReconnecting = false;
        private object _lock = new object();
        private bool _reconnect()
        {
            lock (_lock)
            {
                if (IsShouldDisconnect || IsShutdownReceived)
                {
                    return false;
                }

                while (!IsConnected && !IsShouldDisconnect && !IsShutdownReceived)
                {
                    if (IsConnected)
                    {
                        return true;
                    }
                    try
                    {
                        OnTryReconnect?.Invoke(this);
                        IsReconnecting = true;
                        Connect();
                        IsReconnecting = false;
                        if (IsConnected)
                        {
                            return true;
                        }
                    }
                    catch { }
                    System.Threading.Thread.Sleep((int)TimeSpan.FromSeconds(15).TotalMilliseconds);
                    IsReconnecting = false;
                }
                return false;
            }

        }

        #region Send

        protected async Task SendAsync(ApiRequest obj)
        {

            var data = JsonConvert.SerializeObject(obj);
            await Task.Run(() => _socket.Send(data));
        }

        //public async Task SendWithInterfaceAsync(Expression<Func<TCall, Action>> lambda)
        //{
        //    var method = ((MethodCallExpression)lambda.Body).Method;

        //    var methodName = method.Name;
        //    var model = new ApiRequest(methodName);

        //    await SendAsync(model);
        //}

        //public async Task SendWithInterfaceAsync<TModel>(Expression<Func<TCall, Action<TModel>>> lambda, TModel data)
        //{
        //    var method = ((MethodCallExpression)lambda.Body).Method;

        //    var methodName = method.Name;
        //    var param = method.GetParameters().FirstOrDefault();

        //    var model = new ApiRequest(methodName);
        //    model.SetParam(param.Name, data);

        //    await SendAsync(model);
        //}

        //public async Task SendWithInterfaceAsync<TModel>(Expression<Func<TCall, Action<TModel>>> lambda, Dictionary<string, object> parameters)
        //{
        //    var method = ((MethodCallExpression)lambda.Body).Method;

        //    var methodName = method.Name;

        //    var model = new ApiRequest(methodName);
        //    foreach (var parameter in parameters)
        //    {
        //        model.SetParam(parameter.Key, parameter.Value);
        //    }

        //    await SendAsync(model);
        //}

        //public async Task SendWithInterfaceAsync<TModel>(Expression<Func<TCall, Action<TModel>>> lambda, params object[] parameters)
        //{
        //    var method = ((MethodCallExpression)lambda.Body).Method;

        //    var methodName = method.Name;

        //    var model = new ApiRequest(methodName);
        //    var methodParameters = method.GetParameters();
        //    for (var i = 0; i < methodParameters.Length; i++)
        //    {
        //        var methodParameter = methodParameters[i];
        //        model.SetParam(methodParameter.Name, parameters[i]);
        //    }

        //    await SendAsync(model);
        //}

        #endregion

        #region Events

        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler OnOpen;
        public event VoidEventHandler OnConnected;
        public event VoidEventHandler OnReconnected;
        public event VoidEventHandler OnTryReconnect;
        public event VoidEventHandler OnDisconnected;
        public event SocketEventHandler OnBeforeConnect;

        public delegate void VoidEventHandler(WebSocketClient sender);
        public delegate void SocketEventHandler(WebSocketClient sender, WebSocket socket);

        protected virtual void _onOpen(object sender, EventArgs e)
        {
            OnConnected?.Invoke(this);
            IsConnected = true;

            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer = null;
            }

            _timer = new Timer(10000);

            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            _timer.Interval = 10000;
            _timer.Enabled = true;

            OnOpen?.Invoke(this, e);
        }

        protected virtual void _onError(object sender, ErrorEventArgs e)
        {
            OnError?.Invoke(this, e);
        }

        protected virtual void _onClose(object sender, CloseEventArgs e)
        {
            IsConnected = false;
            OnClose?.Invoke(this, e);
            OnDisconnected?.Invoke(this);
            if (!e.WasClean && !IsShouldDisconnect && !IsReconnecting && !IsConnected)
            {
                if (_reconnect())
                {
                    OnReconnected?.Invoke(this);
                    IsReconnecting = false;
                }
            }
        }

        protected virtual MulticastDelegate GetEventDelegate(FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(this) as MulticastDelegate;
        }

        protected virtual void _onMessage(object sender, MessageEventArgs e)
        {
            OnMessage?.Invoke(this, e);

            if (e.IsPing)
                return;

            var request = JsonConvert.DeserializeObject<ApiRequest>(e.Data);

            if (!string.IsNullOrWhiteSpace(request.RequestId))
            {
                var parameters = request.Params.Select(x => x.Value).Cast<object>().ToArray();
                MessageBus.Invoke(request.RequestId, this, parameters);
            }

            // Events
            try
            {
                var fieldInfo = PropertyHelper
                                .GetFieldInfosIncludingBaseClasses(GetType(), BindingFlags.Instance | BindingFlags.NonPublic)
                                //.GetRuntimeFields() 
                                .FirstOrDefault(x => string.Equals(x.Name, "On" + request.Action));

                if (fieldInfo != null)
                {
                    var parameters = request.Params.Select(x => x.Value).Cast<object>().ToArray();
                    //var eventDelegate = fieldInfo.GetValue(this) as MulticastDelegate;
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

                            if (LockApiRequests)
                            {
                                lock (_syncLock)
                                {
                                    LastReceivedApiRequest = request;
                                    handler.Method.Invoke(handler.Target, typeSafeParameters);
                                }
                            }
                            else
                            {
                                handler.Method.Invoke(handler.Target, typeSafeParameters);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }

            // Receivers
            foreach (var clientReceiver in ClientReceivers)
            {
                clientReceiver.HandleApiRequest(request);
            }
        }

        private object _syncLock = new object();
        public ApiRequest LastReceivedApiRequest { get; private set; }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            try
            {
                if (_socket != null)
                {
                    if (_socket.ReadyState != WebSocketState.Closed && _socket.ReadyState != WebSocketState.Closing)
                    {
                        _socket.CloseAsync();
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Send Helper

        public async Task<WaitForHandler> SendWithHandlerAsync<T>(Expression<Action<T>> expression)
        {
            var requestId = Guid.NewGuid().ToString().Substring(0, 8);
            var waitForHandler = MessageBus.RegisterWaitForHandler(requestId);
            await SendAsync(expression, requestId);
            return waitForHandler;
        }

        public async Task SendAsync<T>(Expression<Action<T>> expression)
        {
            await SendAsync(_dynamicMethodCall(expression, null));
        }

        public async Task SendAsync<T>(Expression<Action<T>> expression, object requestId)
        {
            await SendAsync(_dynamicMethodCall(expression, requestId?.ToString()));
        }

        protected ApiRequest _dynamicMethodCall<T>(Expression<Action<T>> expression, string requestId)
        {
            var body = (MethodCallExpression)expression.Body;

            var request = new ApiRequest(body.Method.Name);
            request.RequestId = requestId;
            var controllerAttr = typeof(T).GetCustomAttribute<ControllerAttribute>();
            if (controllerAttr != null && !string.IsNullOrWhiteSpace(controllerAttr.Name))
            {
                if (controllerAttr.Name.EndsWith("Controller"))
                {
                    request.Controller = controllerAttr.Name.Replace("Controller", "");
                }
                else
                {
                    request.Controller = controllerAttr.Name;
                }
            }

            for (var i = 0; i < body.Arguments.Count; i++)
            {
                var argument = body.Arguments[i];

                if (argument is ConstantExpression)
                {

                }
                var member = _resolveMemberExpression(argument);

                var parameter = body.Method.GetParameters()[i];
                var name = parameter.Name;

                var value = _getValue(member);
                request.SetParam(name, value);
            }
            return request;
        }

        private static MemberExpression _resolveMemberExpression(Expression expression)
        {

            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            else if (expression is UnaryExpression)
            {
                // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
                return (MemberExpression)((UnaryExpression)expression).Operand;
            }
            else
            {
                throw new NotSupportedException(expression.ToString());
            }
        }

        private static object _getValue(MemberExpression exp)
        {
            // expression is ConstantExpression or FieldExpression
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value)
                        .GetType()
                        .GetField(exp.Member.Name)
                        .GetValue(((ConstantExpression)exp.Expression).Value);
            }
            else if (exp.Expression is MemberExpression)
            {
                return _getValue((MemberExpression)exp.Expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Receivers

        public void RegisterReceiver<T>(T clientReceiver)
            where T : WebSocketClientReceiver
        {
            clientReceiver.WebSocketClient = this;
            ClientReceivers.Add(clientReceiver);
        }

        #endregion
    }

    public class WebSocketClient<T> : WebSocketClient
    {
        public WebSocketClient(string url)
            : base(url) { }

        public WebSocketClient(string url, string username, string password, bool preAuth)
            : base(url, username, password, preAuth) { }

        public async Task SendAsync(Expression<Action<T>> expression)
        {
            await SendAsync(_dynamicMethodCall(expression, null));
        }

        public async Task SendAsync(Expression<Action<T>> expression, object requestId)
        {
            await SendAsync(_dynamicMethodCall(expression, requestId?.ToString()));
        }
    }

    public abstract class WebSocketClientReceiver
    {
        public WebSocketClient WebSocketClient { get; internal set; }
        public abstract void HandleApiRequest(ApiRequest apiRequest);
    }
    public class WebSocketClientReceiver<T> : WebSocketClientReceiver
    {
        private Dictionary<string, MethodInfo[]> Methods { get; set; } = new Dictionary<string, MethodInfo[]>();
        public WebSocketClientReceiver()
        {
#warning TODO fehler werden wenn WebSocketClientReceiver<T>  nicht T implementiert

            var controllerName = typeof(T).GetCustomAttribute<ControllerAttribute>()?.Name;
            if (controllerName != null)
            {
                Methods[controllerName] = typeof(T).GetMethods().ToArray();
            }

        }

        public override void HandleApiRequest(ApiRequest apiRequest)
        {
            if (string.IsNullOrWhiteSpace(apiRequest.Controller) || string.IsNullOrWhiteSpace(apiRequest.Action))
            {
                return;
            }

            if (Methods.TryGetValue(apiRequest.Controller, out var methods))
            {
                var method = methods.FirstOrDefault(x => x.Name == apiRequest.Action && x.GetParameters().Length == apiRequest.Params.Count);
                if (method != null)
                {
                    var parameterList = method.GetParameters();
                    var parameters = new object[parameterList.Length];
                    for (var i = 0; i < parameterList.Length; i++)
                    {
                        var parameterInfo = parameterList[i];
                        if (apiRequest.Params.TryGetValue(parameterInfo.Name, out var parameter))
                        {
                            if (parameterInfo.ParameterType == parameter?.GetType())
                            {
                                parameters[i] = parameter;
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    parameters[i] = Convert.ChangeType(parameter, parameterInfo.ParameterType);
                                    continue;
                                }
                                catch { }
                            }
                            try
                            {
                                parameters[i] = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(parameter), parameterInfo.ParameterType);
                            }
                            catch { }
                        }
                    }
                    method.Invoke(this, parameters);
                }
            }
        }
    }
}

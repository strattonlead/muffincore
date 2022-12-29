//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Muffin.Common.Api.WebSockets;
//using Muffin.Common.Util;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Net.WebSockets;
//using System.Reflection;
//using System.Security.Principal;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Muffin.Common.Util.WebSockets
//{
//    public abstract class WebSocketsHandler
//    {
//        public WebSocketsConnectionManager WebSocketsConnectionManager { get; set; }
//        public SubscriptionChannelManager ChannelManager { get; protected set; }
//        protected readonly ILogger Logger;
//        protected readonly IServiceProvider ServiceProvider;

//        public WebSocketsHandler(WebSocketsConnectionManager WebSocketsConnectionManager, IServiceProvider serviceProvider)
//        {
//            WebSocketsConnectionManager = WebSocketsConnectionManager;
//            ServiceProvider = serviceProvider;
//        }

//        public WebSocketsHandler(WebSocketsConnectionManager WebSocketsConnectionManager, ILogger logger, IServiceProvider serviceProvider)
//            : this(WebSocketsConnectionManager, serviceProvider)
//        {
//            Logger = logger;
//        }

//        public virtual void OnConnected(WebSockets socket, HttpContext context)
//        {
//            WebSocketsConnectionManager.AddSocket(socket, context);
//        }

//        public virtual async Task OnDisconnected(WebSockets socket, HttpContext context)
//        {
//            await WebSocketsConnectionManager.RemoveSocket(WebSocketsConnectionManager.GetId(socket), context);
//        }

//        public virtual async Task OnError(WebSockets socket, HttpContext context)
//        {
//            await WebSocketsConnectionManager.RemoveSocket(WebSocketsConnectionManager.GetId(socket), context);
//        }

//        public virtual async Task OnAborted(WebSockets socket, HttpContext context)
//        {
//            await WebSocketsConnectionManager.RemoveSocket(WebSocketsConnectionManager.GetId(socket), context);
//        }

//        #region Connection Handling

//        public virtual async Task CloseConnection(string id, WebSocketsCloseStatus closeStatus = WebSocketsCloseStatus.NormalClosure)
//        {
//            await WebSocketsConnectionManager.RemoveSocket(id, closeStatus);
//        }

//        public virtual async Task CloseConnectionByIdentity(string identity, WebSocketsCloseStatus closeStatus = WebSocketsCloseStatus.NormalClosure)
//        {
//            await WebSocketsConnectionManager.RemoveSocketByIdentity(identity, closeStatus);
//        }

//        public virtual string[] GetConnectionsByIdentity(string identity)
//        {
//            return WebSocketsConnectionManager.GetSocketIdsByIdentity(identity);
//        }

//        public virtual SocketInfo[] GetSocketInfosByIdentity(string identity)
//        {
//            return WebSocketsConnectionManager.GetSocketInfosByIdentity(identity);
//        }

//        public SocketInfo[] GetSocketInfosByState(WebSocketstate state)
//        {
//            return WebSocketsConnectionManager.GetAllSocketInfos()
//                .Where(x => x.Socket.State == state)
//                .ToArray();
//        }

//        public async void RemoveSocket(SocketInfo socketInfo)
//        {
//            await WebSocketsConnectionManager.RemoveSocket(socketInfo.SocketId.ToString(), WebSocketsCloseStatus.NormalClosure);
//        }

//        #endregion

//        #region Plaintext

//        public async Task SendMessageAsync(WebSockets socket, string message)
//        {
//            if (socket == null)
//            {
//                //var id = WebSocketsConnectionManager.GetId(socket);
//                //Logger?.LogInformation($"Socket not found. Remove: {socket}");
//                //await WebSocketsConnectionManager.RemoveSocket(id, null);
//                return;
//            }

//            if (socket.State != WebSocketstate.Open)
//            {
//                Logger?.LogInformation($"Socket closed. Remove: {socket}");
//                return;
//            }

//            var socketInfo = WebSocketsConnectionManager.GetSocketInfoBySocket(socket);
//            if (socketInfo != null)
//            {
//                socketInfo.LastSendDate = DateTime.UtcNow;
//            }

//            var bytes = Encoding.UTF8.GetBytes(message);
//            await socket.SendAsync(buffer: new ArraySegment<byte>(array: bytes,
//                                                                  offset: 0,
//                                                                  count: bytes.Length),
//                                   messageType: WebSocketsMessageType.Text,
//                                   endOfMessage: true,
//                                   cancellationToken: CancellationToken.None);
//        }

//        public async Task SendMessageAsync(string socketId, string message)
//        {
//            await SendMessageAsync(WebSocketsConnectionManager.GetSocketById(socketId), message);
//        }

//        public async Task SendMessageAsync(IEnumerable<string> socketIds, string message)
//        {
//            foreach (var socketId in socketIds)
//                await SendMessageAsync(socketId, message);
//        }

//        public async Task SendMessageToAllAsync(string message)
//        {
//            foreach (var pair in WebSocketsConnectionManager.GetAll())
//            {
//                if (pair.Value.State == WebSocketstate.Open)
//                    await SendMessageAsync(pair.Value, message);
//            }
//        }

//        public async Task SendMessageToPrincipalAsync(IPrincipal principal, string message)
//        {
//            var WebSocketsIds = WebSocketsConnectionManager.GetIds(principal);
//            await SendMessageAsync(WebSocketsIds, message);
//        }

//        public async Task SendMessageToPrincipalsAsync(IEnumerable<IPrincipal> principals, string message)
//        {
//            foreach (var principal in principals)
//                await SendMessageToPrincipalAsync(principal, message);
//        }

//        public async Task SendMessageToIdentityAsync(string identity, string message)
//        {
//            var WebSocketsIds = WebSocketsConnectionManager.GetSocketIdsByIdentity(identity);
//            await SendMessageAsync(WebSocketsIds, message);
//        }

//        public async Task SendMessageToIdentitiesAsync(IEnumerable<string> identities, string message)
//        {
//            foreach (var identity in identities)
//                await SendMessageToIdentityAsync(identity, message);
//        }


//        #endregion

//        #region Objects

//        public async Task SendObjectAsync<T>(string socketId, T obj)
//        {
//            await SendMessageAsync(socketId, _serialize(obj));
//        }

//        public async Task SendObjectAsync<T>(WebSockets socket, T obj)
//        {
//            await SendMessageAsync(socket, _serialize(obj));
//        }

//        public async Task SendObjectAsync<T>(IEnumerable<string> socketIds, T obj)
//        {
//            await SendMessageAsync(socketIds, _serialize(obj));
//        }

//        public async Task SendObjectAsync<T>(IEnumerable<string> socketIds, string action, T obj)
//        {
//            await SendMessageAsync(socketIds, _serialize(obj, action));
//        }

//        public async Task SendObjectToAllAsync<T>(T obj)
//        {
//            await SendMessageToAllAsync(_serialize(obj));
//        }

//        public async Task SendObjectToPrincipalAsync<T>(IPrincipal principal, string action, T obj)
//        {
//            var WebSocketsIds = WebSocketsConnectionManager.GetIds(principal);
//            await SendObjectAsync(WebSocketsIds, action, obj);
//        }

//        public async Task SendObjectToPrincipalAsync<T>(IPrincipal principal, T obj)
//        {
//            var WebSocketsIds = WebSocketsConnectionManager.GetIds(principal);
//            await SendObjectAsync(WebSocketsIds, obj);
//        }

//        public async Task SendObjectToPrincipalAsync<T>(IEnumerable<IPrincipal> principals, T obj)
//        {
//            await SendMessageToPrincipalsAsync(principals, _serialize(obj));
//        }

//        //protected bool ShouldProcessAction(string socketId, string action)
//        //{
//        //    if (ChannelManager == null)
//        //    {
//        //        return true;
//        //    }

//        //    var socketInfo = WebSocketsConnectionManager.GetSocketInfoBySocketId(socketId);
//        //    if (socketInfo == null)
//        //    {
//        //        return true;
//        //    }

//        //    if (socketInfo.Subscriptions == null)
//        //    {
//        //        return false;
//        //    }

//        //    return socketInfo.Subscriptions.Contains(action);
//        //}

//        protected string _serialize<T>(T obj, string action = "", string controller = "", string requestId = "")
//        {
//            try
//            {
//                var settings = new JsonSerializerSettings
//                {
//                    Converters = { new FormatNumbersAsTextConverter() }
//                };
//                if (typeof(T) == typeof(ApiRequest))
//                {
//                    if (!string.IsNullOrWhiteSpace(action))
//                    {
//                        (obj as ApiRequest).Action = action;
//                    }
//                    if (!string.IsNullOrWhiteSpace(controller))
//                    {
//                        (obj as ApiRequest).Controller = controller;
//                    }
//                    if (!string.IsNullOrWhiteSpace(requestId))
//                    {
//                        (obj as ApiRequest).RequestId = requestId;
//                    }
//                    return JsonConvert.SerializeObject(obj, settings);
//                }
//                var response = new ApiRequest(action, controller);
//                response.RequestId = requestId;
//                response.SetParam("model", obj);
//                return JsonConvert.SerializeObject(response, settings);
//            }
//            catch (Exception e)
//            {
//                if (Logger != null)
//                {
//                    Logger.LogError(e.ToString());
//                }
//                throw e;
//            }
//        }

//        internal sealed class FormatNumbersAsTextConverter : JsonConverter
//        {
//            public override bool CanRead => false;
//            public override bool CanWrite => true;
//            public override bool CanConvert(Type type) => type == typeof(long) || type == typeof(long?);

//            public override void WriteJson(
//                JsonWriter writer, object value, JsonSerializer serializer)
//            {
//                if (value != null)
//                {
//                    if (value.GetType() == typeof(long))
//                    {
//                        long number = (long)value;
//                        writer.WriteValue(number.ToString());
//                    }
//                    else if (value.GetType() == typeof(long?))
//                    {
//                        long? number = (long?)value;
//                        writer.WriteValue(number.Value.ToString());
//                    }
//                }
//                else
//                {
//                    writer.WriteValue((string)null);
//                }
//            }

//            public override object ReadJson(
//                JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
//            {
//                throw new NotSupportedException();
//            }
//        }

//        #endregion

//        public virtual async Task ReceiveAsync(WebSockets socket, WebSocketsReceiveResult result, byte[] buffer, HttpContext context)
//        {
//            var socketInfo = WebSocketsConnectionManager.GetSocketInfoBySocket(socket);
//            if (socketInfo != null)
//                socketInfo.LastReceiveDate = DateTime.UtcNow;
//            HandleApiReceive(socket, result, buffer, context);
//        }

//        protected virtual void HandleApiReceive(WebSockets socket, WebSocketsReceiveResult result, byte[] buffer, HttpContext context)
//        {
//            var data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

//            Logger?.LogInformation(data);

//            ApiRequest apiRequest;
//            try
//            {
//                apiRequest = JsonConvert.DeserializeObject<ApiRequest>(data);
//            }
//            catch
//            {
//                Logger?.LogWarning("Unable to deserialize request... Enable Trace to see request details.");
//                return;
//            }

//            var WebSocketsRequest = new WebSocketsRequest()
//            {
//                Action = apiRequest.Action,
//                Controller = apiRequest.Controller,
//                RequestId = apiRequest.RequestId,
//                Principal = context.User,
//                SocketId = WebSocketsConnectionManager.GetId(socket),
//                Context = context,
//                //Result = result,
//                Socket = socket,
//                Handler = this
//            };

//            var responseParameter = new WebSocketsResponse(WebSocketsRequest, this);

//            var mi = GetType()
//                .GetMethods()
//                .FirstOrDefault(x => string.Equals(x.Name, apiRequest.Action));

//            if (mi != null)
//            {
//                var parameterInfos = mi.GetParameters();
//                List<object> parameterList = new List<object>();
//                for (var i = 0; i < parameterInfos.Length; i++)
//                {
//                    var parameterInfo = parameterInfos[i];
//                    if (apiRequest.Params.ContainsKey(parameterInfo.Name))
//                    {
//                        var value = apiRequest.Params[parameterInfo.Name];

//                        // Wenn parameterInfo.ParameterType ein Interface ist => auf ein Objekt umbiegen
//                        var parameterType = parameterInfo.ParameterType;
//                        if (InterfaceToTypeMappings.ContainsKey(parameterType))
//                        {
//                            Logger?.LogInformation($"Switch parameter {parameterInfo.Name} from {parameterType.Name} to {InterfaceToTypeMappings[parameterType].Name}");
//                            parameterType = InterfaceToTypeMappings[parameterType];
//                        }

//                        try
//                        {
//                            if (value is JObject)
//                            {
//                                var typeSaveValue = ((JObject)value).ToObject(parameterType);
//                                parameterList.Add(typeSaveValue);
//                            }
//                            else
//                            {
//                                var typeSaveValue = Convert.ChangeType(value, parameterType);
//                                parameterList.Add(typeSaveValue);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            Logger?.LogWarning(ex.ToString());
//                            parameterList.Add(value);
//                        }
//                    }
//                }


//                InvokeApiMethod(mi, parameterList.ToArray(), WebSocketsRequest, responseParameter);
//            }


//            var fieldInfo = PropertyHelper
//                                .GetFieldInfosIncludingBaseClasses(GetType(), BindingFlags.Instance | BindingFlags.NonPublic)
//                                .FirstOrDefault(x => string.Equals(x.Name, "On" + apiRequest.Action));

//            object[] parameters = null;
//            if (fieldInfo != null)
//            {
//                parameters = apiRequest.Params.Select(x => x.Value).Cast<object>().ToArray();
//                var eventDelegate = fieldInfo.GetValue(this) as MulticastDelegate;
//                if (eventDelegate != null)
//                {
//                    foreach (var handler in eventDelegate.GetInvocationList())
//                    {
//                        var methodParameters = handler.Method.GetParameters();

//                        var typeSafeParameters = new List<object>();

//                        //var typeSafeParameters = new object[methodParameters.Length];
//                        for (var i = 0; i < methodParameters.Length; i++)
//                        {
//                            var methodParameter = methodParameters[i];

//                            if (apiRequest.Params.ContainsKey(methodParameter.Name))
//                            {
//                                var rawValue = apiRequest.Params[methodParameter.Name];
//                                if (rawValue != null)
//                                {
//                                    if (rawValue.GetType() == methodParameter.ParameterType)
//                                    {
//                                        typeSafeParameters.Add(rawValue);
//                                    }
//                                    else
//                                    {
//                                        var jsonValue = JsonConvert.SerializeObject(rawValue);
//                                        var typeSafeValue = JsonConvert.DeserializeObject(jsonValue, methodParameter.ParameterType/*, DeserializableInterfaceJsonHelper.GetInterfaceJsonSerializerSettings()*/);
//                                        typeSafeParameters.Add(typeSafeValue);

//                                    }
//                                }
//                                else
//                                {
//                                    typeSafeParameters.Add(null);
//                                }
//                            }

//                            //var rawValue = parameters[i];
//                            //var jsonValue = JsonConvert.SerializeObject(rawValue);
//                            //var typeSafeValue = JsonConvert.DeserializeObject(jsonValue, methodParameter.ParameterType/*, DeserializableInterfaceJsonHelper.GetInterfaceJsonSerializerSettings()*/);
//                            //typeSafeParameters[i] = typeSafeValue;
//                        }

//                        InvokeApiEvent(handler.Method, handler.Target, typeSafeParameters.ToArray(), WebSocketsRequest, responseParameter);
//                        //handler.Method.Invoke(handler.Target, typeSafeParameters);
//                    }
//                }
//            }

//            WebSocketsController controller = null;
//            if (!string.IsNullOrWhiteSpace(apiRequest.Controller) && !string.IsNullOrWhiteSpace(apiRequest.Action))
//            {
//                var type = _getControllerType(apiRequest.Controller);
//                if (type != null)
//                {
//                    try
//                    {
//                        controller = WebSocketsControllerFactory.CreateController(type, WebSocketsRequest, responseParameter, ServiceProvider);
//                        if (controller != null)
//                        {
//                            try
//                            {
//                                var controllerActions = type.GetMethods()
//                                                                //.Where(x => x.DeclaringType == type)
//                                                                .Where(x => string.Equals(x.Name, apiRequest.Action));

//                                if (controllerActions != null && controllerActions.Any())
//                                {
//                                    foreach (var controllerAction in controllerActions)
//                                    {
//                                        if (controllerAction.GetParameters().Length != apiRequest.Params?.Count)
//                                        {
//                                            continue;
//                                        }

//                                        parameters = _getMethodParameters(controllerAction, apiRequest);
//                                        if (controllerAction.GetParameters().Length != parameters.Count())
//                                        {
//                                            continue;
//                                        }

//                                        var authorizationAttribute = controllerAction.GetCustomAttribute<AuthorizeAttribute>(true);
//                                        if (authorizationAttribute == null)
//                                        {
//                                            authorizationAttribute = controller.GetType().GetCustomAttribute<AuthorizeAttribute>(true);
//                                        }

//                                        var authorizationContext = new AuthorizationContext(context?.User, controllerAction, controller);
//                                        var shouldInvoke = controller.OnAuthorization(authorizationContext);
//                                        if (shouldInvoke)
//                                        {
//                                            controller.OnActionExecuting();
//                                            controllerAction.Invoke(controller, parameters);
//                                            controller.OnActionExecuted();
//                                        }

//                                    }
//                                }
//                            }
//                            catch (Exception e)
//                            {
//                                controller.OnActionError(e);
//                            }

//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        Logger?.LogError($"Unable to instantiate service {type.Name}");
//                        Logger?.LogError($"{e.ToString()}");
//                    }
//                }
//            }

//            // Message Bus mit ControllerContext aufrufen
//            if (!string.IsNullOrWhiteSpace(apiRequest?.RequestId))
//            {
//                var messageBus = MessageBus.GetInstance();
//                messageBus.Invoke(apiRequest.RequestId, parameters);
//            }
//        }

//        private static Dictionary<string, Type> _controllerMap;
//        private static object _controllerMapLock = new object();
//        private Type _getControllerType(string controller)
//        {
//            lock (_controllerMapLock)
//            {
//                if (_controllerMap == null)
//                {
//                    _controllerMap = new Dictionary<string, Type>();
//                    try
//                    {
//                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//                        {
//                            try
//                            {
//                                var types = assembly.GetTypes()
//                                                            .Where(x => typeof(WebSocketsController).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract && x.Name.EndsWith("Controller"))
//                                                            .ToArray();

//                                foreach (Type type in types)
//                                {
//                                    var key = type.Name.Replace("Controller", "");
//                                    _controllerMap[key] = type;
//                                }
//                            }
//                            catch { }
//                        }
//                    }
//                    catch { }

//                }
//            }

//            if (_controllerMap.TryGetValue(controller, out Type result))
//            {
//                return result;
//            }
//            return null;
//        }

//        private object[] _getMethodParameters(MethodInfo mi, ApiRequest apiRequest)
//        {
//            var methodParameters = mi.GetParameters();
//            var typeSafeParameters = new List<object>();

//            //var typeSafeParameters = new object[methodParameters.Length];
//            for (var i = 0; i < methodParameters.Length; i++)
//            {
//                var methodParameter = methodParameters[i];

//                if (apiRequest.Params.ContainsKey(methodParameter.Name))
//                {
//                    var rawValue = apiRequest.Params[methodParameter.Name];
//                    if (rawValue != null)
//                    {
//                        if (rawValue.GetType() == methodParameter.ParameterType)
//                        {
//                            typeSafeParameters.Add(rawValue);
//                        }
//                        else
//                        {
//                            var jsonValue = JsonConvert.SerializeObject(rawValue);
//                            var typeSafeValue = JsonConvert.DeserializeObject(jsonValue, methodParameter.ParameterType/*, DeserializableInterfaceJsonHelper.GetInterfaceJsonSerializerSettings()*/);
//                            typeSafeParameters.Add(typeSafeValue);

//                        }
//                    }
//                    else
//                    {
//                        typeSafeParameters.Add(null);
//                    }
//                }
//                else
//                {
//                    typeSafeParameters.Add(null);
//                }

//                //var rawValue = parameters[i];
//                //var jsonValue = JsonConvert.SerializeObject(rawValue);
//                //var typeSafeValue = JsonConvert.DeserializeObject(jsonValue, methodParameter.ParameterType/*, DeserializableInterfaceJsonHelper.GetInterfaceJsonSerializerSettings()*/);
//                //typeSafeParameters[i] = typeSafeValue;
//            }

//            return typeSafeParameters.ToArray();
//        }

//        protected virtual void InvokeApiEvent(MethodInfo mi, object target, object[] parameters, WebSocketsRequest requestParameter, WebSocketsResponse responseParameter)
//        {
//            mi.Invoke(target, parameters);
//        }

//        protected virtual void InvokeApiMethod(MethodInfo mi, object[] parameters, WebSocketsRequest requestParameter, WebSocketsResponse responseParameter)
//        {
//            mi.Invoke(this, parameters);
//        }

//        protected Dictionary<Type, Type> InterfaceToTypeMappings = new Dictionary<Type, Type>();
//        public void RegisterInterface<TInterface, TImplementation>()
//            where TImplementation : new()
//        {
//            if (!typeof(TInterface).IsInterface)
//            {
//                throw new ArgumentException($"{nameof(TInterface)} must be an interface!");
//            }
//            if (typeof(TImplementation).IsInterface)
//            {
//                throw new ArgumentException($"{nameof(TImplementation)} must be a class!");
//            }
//            InterfaceToTypeMappings[typeof(TInterface)] = typeof(TImplementation);
//        }

//        public void RegisterInterfaces(IDictionary<Type, Type> typeMappings)
//        {
//            foreach (var pair in typeMappings)
//            {
//                InterfaceToTypeMappings[pair.Key] = pair.Value;
//            }
//        }

//        #region 

//        protected ApiRequest _dynamicMethodCall<T>(Expression<Action<T>> expression)
//        {
//            try
//            {
//                var body = (MethodCallExpression)expression.Body;

//                var request = new ApiRequest(body.Method.Name); // TODO hier evtl noch was für den Controller bauen oder im interface angeben welcher Controller
//                for (var i = 0; i < body.Arguments.Count; i++)
//                {
//                    var argument = body.Arguments[i];

//                    if (argument is ConstantExpression)
//                    {

//                    }
//                    var member = _resolveMemberExpression(argument);

//                    var parameter = body.Method.GetParameters()[i];
//                    var name = parameter.Name;

//                    var value = _getValue(member);
//                    request.SetParam(name, value);
//                }
//                return request;
//            }
//            catch (Exception e)
//            {
//                if (Logger != null)
//                {
//                    Logger.LogError(e.ToString());
//                }
//                throw e;
//            }
//        }

//        private static MemberExpression _resolveMemberExpression(Expression expression)
//        {

//            if (expression is MemberExpression)
//            {
//                return (MemberExpression)expression;
//            }
//            else if (expression is UnaryExpression)
//            {
//                // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
//                return (MemberExpression)((UnaryExpression)expression).Operand;
//            }
//            else
//            {
//                throw new NotSupportedException(expression.ToString());
//            }
//        }

//        private static object _getValue(MemberExpression exp)
//        {
//            // expression is ConstantExpression or FieldExpression
//            if (exp.Expression is ConstantExpression)
//            {
//                return (((ConstantExpression)exp.Expression).Value)
//                        .GetType()
//                        .GetField(exp.Member.Name)
//                        .GetValue(((ConstantExpression)exp.Expression).Value);
//            }
//            else if (exp.Expression is MemberExpression)
//            {
//                return _getValue((MemberExpression)exp.Expression);
//            }
//            else
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public virtual async Task InvokeAll<T>(Expression<Action<T>> expression)
//        {
//            await SendMessageToAllAsync(_serialize(_dynamicMethodCall(expression)));
//        }

//        public virtual async Task Invoke<T>(string socketId, Expression<Action<T>> expression)
//        {
//            await SendMessageAsync(socketId, _serialize(_dynamicMethodCall(expression)));
//        }

//        public virtual async Task Invoke<T>(string socketId, Expression<Action<T>> expression, string requestId)
//        {
//            await SendMessageAsync(socketId, _serialize(_dynamicMethodCall(expression), "", "", requestId));
//        }

//        public virtual async Task Invoke<T>(WebSockets socket, Expression<Action<T>> expression)
//        {
//            await SendMessageAsync(socket, _serialize(_dynamicMethodCall(expression)));
//        }

//        public virtual async Task Invoke<T>(WebSockets socket, Expression<Action<T>> expression, string requestId)
//        {
//            await SendMessageAsync(socket, _serialize(_dynamicMethodCall(expression), "", "", requestId));
//        }

//        public virtual async Task Invoke<T>(IPrincipal principal, Expression<Action<T>> expression)
//        {
//            await SendMessageToPrincipalAsync(principal, _serialize(_dynamicMethodCall(expression)));
//        }

//        public virtual async Task Invoke<T>(IEnumerable<IPrincipal> principals, Expression<Action<T>> expression)
//        {
//            await SendMessageToPrincipalsAsync(principals, _serialize(_dynamicMethodCall(expression)));
//        }

//        public virtual async Task InvokeWithIdentity<T>(string identity, Expression<Action<T>> expression)
//        {
//            await SendMessageToIdentityAsync(identity, _serialize(_dynamicMethodCall(expression)));
//        }

//        public virtual async Task InvokeWithIdentities<T>(IEnumerable<string> identities, Expression<Action<T>> expression)
//        {
//            await SendMessageToIdentitiesAsync(identities, _serialize(_dynamicMethodCall(expression)));
//        }

//        #endregion
//    }

//    public abstract class WebSocketsHandler<T> : WebSocketsHandler
//    {
//        #region Constructor

//        public WebSocketsHandler(WebSocketsConnectionManager WebSocketsConnectionManager, IServiceProvider serviceProvider)
//            : base(WebSocketsConnectionManager, serviceProvider) { }

//        public WebSocketsHandler(WebSocketsConnectionManager WebSocketsConnectionManager, ILogger logger, IServiceProvider serviceProvider)
//            : base(WebSocketsConnectionManager, logger, serviceProvider) { }

//        #endregion

//        #region

//        public async Task InvokeAll(Expression<Action<T>> expression)
//        {
//            await SendMessageToAllAsync(_serialize(_dynamicMethodCall(expression)));
//        }

//        public async Task Invoke(string socketId, Expression<Action<T>> expression)
//        {
//            await SendMessageAsync(socketId, _serialize(_dynamicMethodCall(expression)));
//        }

//        public async Task Invoke(WebSockets socket, Expression<Action<T>> expression)
//        {
//            await SendMessageAsync(socket, _serialize(_dynamicMethodCall(expression)));
//        }

//        public async Task Invoke(IPrincipal principal, Expression<Action<T>> expression)
//        {
//            await SendMessageToPrincipalAsync(principal, _serialize(_dynamicMethodCall(expression)));
//        }

//        public async Task Invoke(IEnumerable<IPrincipal> principals, Expression<Action<T>> expression)
//        {
//            await SendMessageToPrincipalsAsync(principals, _serialize(_dynamicMethodCall(expression)));
//        }

//        public async Task InvokeWithIdentity(string identity, Expression<Action<T>> expression)
//        {
//            await SendMessageToIdentityAsync(identity, _serialize(_dynamicMethodCall(expression)));
//        }

//        public async Task InvokeWithIdentities(IEnumerable<string> identities, Expression<Action<T>> expression)
//        {
//            await SendMessageToIdentitiesAsync(identities, _serialize(_dynamicMethodCall(expression)));
//        }

//        #endregion
//    }

//    public class WebSocketsRequest
//    {
//        public string Action { get; set; }
//        public string Controller { get; set; }
//        public string RequestId { get; set; }
//        public IPrincipal Principal { get; set; }
//        public string SocketId { get; set; }
//        public WebSockets Socket { get; set; }
//        //public WebSocketsReceiveResult Result { get; set; }
//        public HttpContext Context { get; set; }
//        public WebSocketsHandler Handler { get; set; }

//        public WebSocketsRequest MakeGeneric(object model)
//        {
//            var modelType = model.GetType();
//            var type = typeof(WebSocketsRequest<>)
//                .MakeGenericType(modelType);
//            var genericRequest = Activator.CreateInstance(type);
//            PropertyHelper.CopyProperties(this, genericRequest);

//            var pi = type.GetProperties().First(x => string.Equals(x.Name, "Model"));
//            pi.SetValue(genericRequest, model);

//            return genericRequest as WebSocketsRequest;
//        }
//    }

//    public class WebSocketsRequest<T> : WebSocketsRequest
//    {
//        public T Model { get; set; }
//    }

//    public class WebSocketsResponse
//    {
//        protected WebSocketsHandler Handler { get; set; }
//        protected WebSockets Socket { get; set; }
//        protected WebSocketsRequest WebSocketsRequest { get; set; }

//        public WebSocketsResponse(WebSocketsRequest WebSocketsRequest, WebSocketsHandler handler)
//        {
//            Socket = WebSocketsRequest?.Socket;
//            WebSocketsRequest = WebSocketsRequest;
//            Handler = handler;
//        }

//        public async void SendMessageAsync(string message)
//        {
//            await Handler.SendMessageAsync(Socket, message);
//        }

//        public async void SendObjectAsync(object obj)
//        {
//            if (obj is ApiRequest)
//            {
//                await Handler.SendObjectAsync(Socket, obj);
//            }
//            else if (WebSocketsRequest != null)
//            {
//                var apiRequest = new ApiRequest(WebSocketsRequest.Action, WebSocketsRequest.Controller, WebSocketsRequest.RequestId);
//                apiRequest.SetParam("model", obj);
//                await Handler.SendObjectAsync(Socket, apiRequest);
//            }
//            else
//            {
//                await Handler.SendObjectAsync(Socket, obj);
//            }
//        }
//    }
//}

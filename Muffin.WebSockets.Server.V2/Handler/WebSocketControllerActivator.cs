using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muffin.Common.Api.WebSockets;
using Muffin.Common.Util;
using Muffin.WebSockets.Server.Controllers;
using Muffin.WebSockets.Server.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Handler
{
    public static class WebSocketControllerActivator
    {
        public static async Task HandleWebSocketRequest(ApiRequest apiRequest, WebSocketRequest webSocketRequest, WebSocketResponse webSocketResponse, IServiceProvider serviceProvider)
        {
            var scopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var logger = serviceProvider.GetService<ILogger<WebSocketRequest>>();
                logger?.LogInformation($"HandleWebSocketRequest -> Controller: {apiRequest?.Controller} Action: {apiRequest?.Action} RequestId: {apiRequest.RequestId} Parameters: {apiRequest?.Params?.Count}");

                WebSocketController controller = null;
                if (!string.IsNullOrWhiteSpace(apiRequest.Controller) && !string.IsNullOrWhiteSpace(apiRequest.Action))
                {
                    var type = _getControllerType(apiRequest.Controller);
                    if (type != null)
                    {
                        try
                        {
                            logger?.LogInformation($"Controller Type found: {type.AssemblyQualifiedName}");
                            var controllerActions = type.GetMethods()
                                                        .Where(x => string.Equals(x.Name, apiRequest.Action))
                                                        .ToArray();

                            //                            MethodInfo controllerAction = null;
                            //                            object[] parameters = null;
                            //                            foreach (var ca in controllerActions)
                            //                            {
                            //                                if (ca.GetParameters().Length != apiRequest.Params?.Count)
                            //                                {
                            //                                    continue;
                            //                                }

                            //                                parameters = _getMethodParameters(ca, apiRequest);
                            //                                if (ca.GetParameters().Length != parameters.Count())
                            //                                {
                            //                                    continue;
                            //                                }

                            //#warning Hier noch was bauen das er die bestmöglichste Methode sucht dier er aufrufen kann. Wenn man mehr parameter schickt, kann man ja die überflüssigen weglassen.

                            //                                controllerAction = ca;
                            //                                break;
                            //                            }


                            ControllerActionMap[] actionMaps = controllerActions
                                .Select(ca =>
                                {
                                    var result = new ControllerActionMap();
                                    result.ControllerAction = ca;

                                    if (ca.GetParameters().Length != apiRequest.Params?.Count)
                                    {
                                        result.FittingType |= ControllerActionFittingType.ParameterCountMismatch;
                                    }

                                    if (ca.GetParameters().Length == 0 && apiRequest.Params?.Count > 0)
                                    {
                                        result.FittingType |= ControllerActionFittingType.NoParameter;
                                        return result;
                                    }

                                    result.Parameters = _getMethodParameters(ca, apiRequest);
                                    if (ca.GetParameters().Length == result.Parameters?.Count())
                                    {
                                        result.FittingType |= ControllerActionFittingType.Perfect;
#warning TODO -> Hier müssen noch die Typen abgeklärt werden!!! Sonst funktioniert das ggf auch bei der App nicht mehr!!!
                                    }
                                    else
                                    {
                                        result.FittingType |= ControllerActionFittingType.ParameterCountMismatch;
                                    }

                                    // Wenn die Parameter nicht passen, versuchen ob man das noch wrappen kann oder ausgepackt werden muss
                                    if (result.FittingType.HasFlag(ControllerActionFittingType.ParameterCountMismatch) && ca.GetParameters().Length < apiRequest.Params?.Count)
                                    {
                                        var methodParameters = ca.GetParameters();
                                        var wrappedParameters = methodParameters.Select(x =>
                                        {
                                            // Erst schauen ob ggf ein Parameter schon passt
                                            if (apiRequest.Params.TryGetValue(x.Name, out object value))
                                            {
                                                if (value.GetType() == x.ParameterType)
                                                {
                                                    return value;
                                                }

                                                var jsonValue = JsonConvert.SerializeObject(value);
                                                return JsonConvert.DeserializeObject(jsonValue, x.ParameterType, new IdToStringConverter());
                                            }

                                            // wenn nicht instlziieren
                                            var properties = x.ParameterType.GetProperties();
                                            if (properties.Any(x => apiRequest.Params.ContainsKey(x.Name)))
                                            {
                                                var temp = Activator.CreateInstance(x.ParameterType);
                                                foreach (var property in properties)
                                                {
                                                    if (apiRequest.Params.TryGetValue(property.Name, out value))
                                                    {
                                                        var typesafeValue = value;
                                                        if (value?.GetType() != property.PropertyType)
                                                        {
                                                            var jsonValue = JsonConvert.SerializeObject(value);
                                                            typesafeValue = JsonConvert.DeserializeObject(jsonValue, property.PropertyType, new IdToStringConverter());
                                                        }

                                                        property.SetValue(temp, typesafeValue);

                                                    }
                                                }
                                                return temp;
                                            }

                                            return null;
                                        }).Cast<object>().ToArray();

                                        // wenn das passt dann rein da
                                        if (wrappedParameters?.Length == ca.GetParameters().Length)
                                        {
                                            result.FittingType |= ControllerActionFittingType.ParameterWrapped;
                                        }
                                    }

                                    return result;
                                })
                                .ToArray();

                            foreach (var map in actionMaps)
                            {
                                var fittingTypes = EnumHelper.SplitEnum(map.FittingType);
                                foreach (var fittingType in fittingTypes)
                                {
                                    logger?.LogInformation($"Action Map -> {fittingType} -> {map.ControllerAction?.Name}");
                                }

                            }

                            var actionMap = actionMaps.FirstOrDefault(x => x.FittingType.HasFlag(ControllerActionFittingType.Perfect));
                            var controllerAction = actionMap?.ControllerAction;

                            logger?.LogInformation($"Controller Action found? -> {controllerAction?.Name}");

                            var controllerFactory = scope.ServiceProvider.GetService<IControllerFactory>();

                            var routeData = new RouteData();
                            routeData.Values.Add("action", apiRequest.Action);
                            routeData.Values.Add("controller", apiRequest.Controller);
                            var actionDescriptor = new ControllerActionDescriptor();
                            actionDescriptor.MethodInfo = controllerAction;
                            actionDescriptor.ControllerTypeInfo = type.GetTypeInfo();

                            var actionContext = new ActionContext(webSocketRequest.Context, routeData, actionDescriptor);
                            var controllerContext = new ControllerContext(actionContext);

                            logger?.LogInformation($"controllerFactory.CreateController");

                            controller = (WebSocketController)controllerFactory.CreateController(controllerContext);
                            controller.WebSocketRequest = webSocketRequest;
                            controller.WebSocketResponse = webSocketResponse;
                            controller.Init();
                            logger?.LogInformation($"controller = null? {controller == null}");
                            logger?.LogInformation($"controllerAction = null? {controllerAction == null}");

                            //controller = WebSocketControllerFactory.CreateController(type, WebSocketRequest, WebSocketResponse, ServiceProvider);
                            try
                            {
                                if (controller != null && controllerAction != null)
                                {
                                    var authorizationContext = new Security.AuthorizationContext(webSocketRequest?.Context?.User, controllerAction, controller);
                                    var shouldInvoke = controller.OnAuthorization(authorizationContext);
                                    logger?.LogInformation($"controller.OnAuthorization authorized? {shouldInvoke}");
                                    if (shouldInvoke)
                                    {
                                        logger?.LogInformation("OnActionExecuting");
                                        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionMap.Parameters, controller);
                                        controller.OnActionExecuting(actionExecutingContext);
                                        logger?.LogInformation($"Invoke {apiRequest?.Action}");
                                        var controllerResult = controllerAction.Invoke(controller, actionMap?.Parameters?.Values?.ToArray());
                                        logger?.LogInformation($"Has Controller Result: {controllerResult != null}");

                                        var actionTimeoutAttribute = controllerAction.GetCustomAttribute<ActionTimeoutAttribute>(true);
                                        if (actionTimeoutAttribute == null)
                                        {
                                            actionTimeoutAttribute = controller.GetType().GetCustomAttribute<ActionTimeoutAttribute>(true);
                                        }

                                        IActionResult actionResult = null;
                                        if (controllerResult is Task<IActionResult>)
                                        {
                                            logger?.LogInformation("Async Result found");
                                            if (actionTimeoutAttribute != null)
                                            {
                                                logger?.LogInformation($"ActionTimeoutAttribute found -> Timeout: {actionTimeoutAttribute.Timeout} CancelByApplicationStopping: {actionTimeoutAttribute.CancelByApplicationStopping}");
                                                var task = (Task<IActionResult>)controllerResult;

                                                var appLifetime = serviceProvider.GetService<IApplicationLifetime>();
                                                if (actionTimeoutAttribute.CancelByApplicationStopping && appLifetime != null)
                                                {
                                                    if (actionTimeoutAttribute.Timeout.HasValue)
                                                    {
                                                        logger?.LogInformation($"Wait {actionTimeoutAttribute.Timeout.Value} ms. Cancel if neccesary");
                                                        task.Wait(actionTimeoutAttribute.Timeout.Value, appLifetime.ApplicationStopping);
                                                        logger?.LogInformation($"Wait done!");
                                                    }
                                                    else
                                                    {
                                                        logger?.LogInformation($"Wait unlimited. Cancel if neccesary");
                                                        task.Wait(appLifetime.ApplicationStopping);
                                                        logger?.LogInformation($"Wait done!");
                                                    }
                                                }
                                                else
                                                {
                                                    if (actionTimeoutAttribute.Timeout.HasValue)
                                                    {
                                                        logger?.LogInformation($"Wait {actionTimeoutAttribute.Timeout.Value} ms. No canel.");
                                                        task.Wait(actionTimeoutAttribute.Timeout.Value);
                                                        logger?.LogInformation($"Wait done!");
                                                    }
                                                    else
                                                    {
                                                        logger?.LogInformation($"Wait unlimited. No canel.");
                                                        await task;
                                                        logger?.LogInformation($"Wait done!");
                                                    }
                                                }

                                                if (task.IsCompleted)
                                                {
                                                    logger?.LogInformation($"Task IsCompleted");
                                                    actionResult = task.Result;
                                                }
                                                else
                                                {
                                                    logger?.LogError($"Task was not completed! Action: {apiRequest?.Action} RID: {apiRequest?.RequestId}");
                                                }
                                            }
                                            else
                                            {
                                                logger?.LogInformation($"Result Type: {controllerResult?.GetType()}");
                                                actionResult = await (Task<IActionResult>)controllerResult;
                                                logger?.LogInformation($"Result Type: {actionResult?.GetType()}");
                                                logger?.LogInformation($"Result Type: {actionResult?.GetType()?.BaseType}");
                                            }
                                        }
                                        else if (controllerResult != null && controllerResult is IActionResult)
                                        {
                                            actionResult = (IActionResult)controllerResult;
                                        }

                                        if (actionResult != null)
                                        {
                                            logger?.LogInformation($"IActionResult ExecuteResultAsync");
                                            await actionResult.ExecuteResultAsync(actionContext);
                                            logger?.LogInformation($"IActionResult Executed {apiRequest?.Action}");
                                        }

                                        logger?.LogInformation($"OnActionExecuted {apiRequest?.Action}");
                                        controller.OnActionExecuted();
                                    }
                                }
                            }
                            catch (Exception e2)
                            {
                                logger?.LogError($"Exception on executing action {apiRequest?.Action}");
                                logger?.LogError(e2.ToString());

                                var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>());
                                exceptionContext.Exception = e2;

                                controller.OnException(exceptionContext);

                                if (exceptionContext.Result != null)
                                {
                                    logger?.LogInformation($"IActionResult EXCEPTION ExecuteResultAsync");
                                    await exceptionContext.Result.ExecuteResultAsync(actionContext);
                                    logger?.LogInformation($"IActionResult EXCEPTION Executed");
                                }
                            }

                            logger?.LogInformation($"ReleaseController");
                            controllerFactory?.ReleaseController(controllerContext, controller);
                        }
                        catch (Exception e)
                        {
                            logger?.LogError(e.ToString());
                            throw e;
                        }
                    }
                }
            }
        }

        private static Dictionary<string, Type> _controllerMap;
        private static object _controllerMapLock = new object();
        private static Type _getControllerType(string controller)
        {
            lock (_controllerMapLock)
            {
                if (_controllerMap == null)
                {
                    _controllerMap = new Dictionary<string, Type>();
                    try
                    {
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                var types = assembly
                                    .GetTypes()
                                    .Where(x => typeof(WebSocketController).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract && x.Name.EndsWith("Controller"))
                                    .ToArray();

                                foreach (Type type in types)
                                {
                                    var key = type.Name.Replace("Controller", "");
                                    _controllerMap[key] = type;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }

            if (_controllerMap.TryGetValue(controller, out Type result))
            {
                return result;
            }
            return null;
        }

        private static Dictionary<string, object> _getMethodParameters(MethodInfo mi, ApiRequest apiRequest)
        {
            var methodParameters = mi.GetParameters();
            var typeSafeParameters = new Dictionary<string, object>();

            //var typeSafeParameters = new object[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterInfo = methodParameters[i];

                if (apiRequest.Params.ContainsKey(parameterInfo.Name))
                {
                    var rawValue = apiRequest.Params[parameterInfo.Name];
                    if (rawValue != null)
                    {
                        if (rawValue.GetType() == parameterInfo.ParameterType)
                        {
                            typeSafeParameters[parameterInfo.Name] = rawValue;
                        }
                        else
                        {
                            var jsonValue = JsonConvert.SerializeObject(rawValue);

                            var typeSafeValue = JsonConvert.DeserializeObject(jsonValue, parameterInfo.ParameterType, new IdToStringConverter());
                            typeSafeParameters[parameterInfo.Name] = typeSafeValue;
                        }
                    }
                    else
                    {
                        typeSafeParameters[parameterInfo.Name] = null;
                    }
                }
                else
                {
                    typeSafeParameters[parameterInfo.Name] = null;
                }
            }

            return typeSafeParameters;
        }
    }

    public class ControllerActionMap
    {
        public ControllerActionFittingType FittingType { get; set; }
        public MethodInfo ControllerAction { get; set; }
        //public object[] Parameters { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    [Flags]
    public enum ControllerActionFittingType
    {
        Perfect = 1,
        ParameterCountMismatch = 2,
        ParameterWrapped = 4,
        NoParameter = 8,
        NoMatch = 16
    }

    public class ActionTimeoutAttribute : Attribute
    {
        /// <summary>
        /// Timeout in Millisekunden
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// Wenn das auf true steht, dann wird der Request bei APP stopping abgebrochen
        /// </summary>
        public bool CancelByApplicationStopping { get; set; }

        public ActionTimeoutAttribute(bool cancelByApplicationStopping)
        {
            Timeout = null;
            CancelByApplicationStopping = cancelByApplicationStopping;
        }

        public ActionTimeoutAttribute(int timeout)
        {
            Timeout = timeout;
            CancelByApplicationStopping = true;
        }

        public ActionTimeoutAttribute(int timeout, bool cancelByApplicationStopping)
        {
            Timeout = timeout;
            CancelByApplicationStopping = cancelByApplicationStopping;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Common.Api.Rest;
using Muffin.Common.Api.WebSockets;
using Muffin.Mvc.Helpers;
using Muffin.WebSockets.Server.Models;
using Muffin.WebSockets.Server.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Controllers
{
    public abstract class WebSocketController : Controller
    {
        #region Properties

        public WebSocket WebSocket { get { return WebSocketRequest?.Socket; } }
        public WebSocketRequest WebSocketRequest { get; internal set; }
        public WebSocketResponse WebSocketResponse { get; internal set; }

        protected bool IsWebSocketRequest { get { return WebSocket != null; } }

        protected readonly ILogger Logger;
        protected readonly WebSocketSubscriptionService WebSocketSubscriptionService;
        protected readonly IWebSocketContextAccessor WebSocketContextAccessor;

        #endregion

        #region Constructor

        public WebSocketController(IServiceProvider serviceProvider)
        {
            Logger = serviceProvider.GetService<ILogger<WebSocketController>>();
            WebSocketSubscriptionService = serviceProvider.GetService<WebSocketSubscriptionService>();
            WebSocketContextAccessor = serviceProvider.GetService<IWebSocketContextAccessor>();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual void Init() { }

        #endregion

        #region Action Result Types

        protected virtual IActionResult ApiSuccess()
        {
            return ApiResult(new { success = true });
        }

        protected virtual IActionResult ApiResult()
        {
            return ApiResult<object>(null, null, null);
        }

        protected virtual IActionResult ScriptResult(string script)
        {
            return ApiResult<object>(null, script);
        }

        protected virtual IActionResult ApiResult<T>(T obj)
        {
            return ApiResult(obj, null, null);
        }

        protected virtual IActionResult ApiResult<T>(T obj, IEnumerable<JsonConverter> jsonConverters)
        {
            return ApiResult(obj, null, jsonConverters);
        }

        protected virtual IActionResult ApiResult<T>(T obj, string script)
        {
            return ApiResult(obj, script, null);
        }

        protected virtual IActionResult ApiResult<T>(T obj, string script, IEnumerable<JsonConverter> jsonConverters)
        {
            Logger?.LogInformation($"Execute ApiResult IsWebSocketsRequest={IsWebSocketRequest}");
            if (IsWebSocketRequest)
            {
                if (obj != null)
                {
                    ApiResponse.SetParam("data", obj);
                }
                else
                {
                    ApiResponse.SetParam("data", new { success = true });
                }

                if (!string.IsNullOrWhiteSpace(script))
                {
                    ApiResponse.SetParam("js", script);
                }

                if (Notifications?.Any() ?? false)
                {
                    ApiResponse.SetParam("notifications", Notifications);
                }

                if (AppStateChanges != null)
                {
                    ApiResponse.SetParam("appState", AppStateChanges);
                }

                Logger?.LogInformation($"ApiResult with App State change?={AppStateChanges != null}");
                return WebSocketsResult();
            }

#warning TODO hier muss das identisch sein wie beim Socket!
            return ControllerHelper.JsonResult(this, new ApiResponse<T>()
            {
                Data = obj,
                Script = script,
                Notifications = Notifications,
                AppStateChanges = AppStateChanges
            }, jsonConverters);
        }

        protected virtual IActionResult ApiJson(object obj)
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(obj, settings),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        protected virtual IActionResult ApiError(Exception e)
        {
            if (IsWebSocketRequest)
            {
                if (e != null)
                {
                    ApiResponse.SetParam("error", e.Message);
                }

                return WebSocketsResult();
            }

            return ControllerHelper.JsonResult(this, new ApiResponse<object>()
            {
                ErrorMessage = e.Message,
                ErrorDetails = e.StackTrace
            });
        }

        protected IActionResult ApiError(string errorMessage, int? errorCode = null)
        {
            if (IsWebSocketRequest)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    ApiResponse.SetParam("error", errorMessage);
                }

                if (errorCode.HasValue)
                {
                    errorCode = 500;
                }

                ApiResponse.SetParam("status", errorCode.ToString());

                return WebSocketsResult();
            }

            return ControllerHelper.JsonResult(this, new ApiResponse<object>()
            {
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            });
        }

        protected IActionResult ApiWarning(string errorMessage, int? errorCode = null)
        {
            if (IsWebSocketRequest)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    ApiResponse.SetParam("error", errorMessage);
                }

                if (errorCode.HasValue)
                {
                    errorCode = 500;
                }

                ApiResponse.SetParam("status", errorCode.ToString());

                return WebSocketsResult();
            }

            SetNotification(new Notification()
            {
                Type = "warning",
                Text = errorMessage
            });
            return ControllerHelper.JsonResult(this, new ApiResponse<object>()
            {
                ErrorCode = errorCode,
                Notifications = Notifications
            });
        }

        protected IActionResult WebSocketsResult()
        {
            Logger?.LogInformation($"Execute WebSocketsResult");
            return new WebSocketActionResult(WebSocketResponse, ApiResponse, Logger);
        }

        #endregion

        #region Actions

        protected object AppStateChanges { get; private set; }
        private object _appStateLock = new object();
        protected void SetAppStateChange(object change)
        {
            lock (_appStateLock)
            {
                AppStateChanges = change;
            }
        }

        private List<Notification> Notifications = new List<Notification>();
        protected void SetNotification(Notification notification)
        {
            if (notification == null)
            {
                return;
            }

            Notifications.Add(notification);
        }

        protected void SetNotification(string text, string type, string color)
        {
            SetNotification(new Notification()
            {
                Text = text,
                Color = color,
                Type = type
            });
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        //public void RegisterInterface<TInterface, TImplementation>()
        //     where TImplementation : new()
        //{
        //    WebSocketRequest?.Handler?.RegisterInterface<TInterface, TImplementation>();
        //}

        //[ApiExplorerSettings(IgnoreApi = true)]
        //public void RegisterInterfaces(IDictionary<Type, Type> typeMappings)
        //{
        //    WebSocketRequest?.Handler?.RegisterInterfaces(typeMappings);
        //}

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual bool OnAuthorization(Security.AuthorizationContext context)
        {
            if (context?.AllowAnonymous != null)
            {
                return true;
            }

            //if (context.Authorize != null)
            //{

            //}

            var authenticated = context?.Principal?.Identity?.IsAuthenticated;
            if (!authenticated.HasValue)
            {
                Logger?.LogInformation($"OnAuthorization context.Principal.Identity is null");
                return false;
            }

            if (!authenticated.Value)
            {
                Logger?.LogInformation($"OnAuthorization context.Principal.Identity.IsAuthenticated is false");
                return false;
            }

            if (context?.RequiredRoles != null)
            {
                if (context.RequiredRoles.Length > 0)
                {
                    foreach (var role in context.RequiredRoles)
                    {
                        var isInRole = context?.Principal?.IsInRole(role);
                        if (isInRole.HasValue && isInRole.Value)
                        {
                            return true;
                        }
                    }
                    Logger?.LogInformation($"OnAuthorization does not have a required role. required {string.Join(",", context.RequiredRoles)}");
                    return false;
                }
            }

            return true;
        }

#warning TODO das hier muss noch anders ablaufen  OnActionExecuting(ActionExecutingContext context)
        //public virtual void OnActionExecuting(ActionExecutingContext context) { }
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual void OnActionExecuted() { }

        //[ApiExplorerSettings(IgnoreApi = true)]
        //public virtual void OnActionError(Exception exception)
        //{  }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual void OnException(ExceptionContext exceptionContext)
        {
            try
            {
                if (IsWebSocketRequest)
                {
                    exceptionContext.Result = ApiError(exceptionContext.Exception);
                }
            }
            catch { }
        }

        #endregion

        #region Response Helpers

        private ApiRequest _response = null;
        private static object _responseLock = new object();
        protected ApiRequest ApiResponse
        {
            get
            {
                lock (_responseLock)
                {
                    if (_response == null)
                    {
                        _response = new ApiRequest()
                        {
                            RequestId = WebSocketRequest?.RequestId,
                            Controller = WebSocketRequest?.Controller,
                            Action = WebSocketRequest?.Action
                        };

                        if (AppStateChanges != null)
                        {
                            _response.SetParam("appState", AppStateChanges);
                        }
                    }
                }

                return _response;
            }
            set { _response = value; }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task SendResponseAsync<T>(Expression<Action<T>> expression)
        {
            var apiRequest = ApiRequestHelper.FromExpression(expression, WebSocketRequest?.RequestId);
            await SendResponseAsync(apiRequest);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task SendResponseAsync(ApiRequest apiRequest)
        {
            if (WebSocketResponse != null)
            {
                await WebSocketResponse.SendObjectAsync(apiRequest);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task SendResponseAsync<T>(T obj)
        {
            await SendResponseDictAsync(new Dictionary<string, object>() { { "data", obj } });
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task SendResponseDictAsync(IDictionary<string, object> parameters)
        {
            var model = ApiResponse;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    model.SetParam(parameter.Key, parameter.Value);
                }
            }

            if (WebSocketResponse != null && model != null)
            {
                await WebSocketResponse?.SendObjectAsync(model);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task SendErrorAsync(Exception e)
        {
            if (WebSocketResponse != null && e != null)
            {
                var apiRequest = new ApiRequest()
                {
                    Action = WebSocketRequest.Action,
                    Controller = WebSocketRequest.Controller,
                    RequestId = WebSocketRequest.RequestId
                };

                if (e != null)
                {
                    apiRequest.SetParam("error", e.Message);
                }

                await WebSocketResponse?.SendObjectAsync(apiRequest);
            }
        }

        #endregion
    }

    public interface ISubscriptionController
    {
        IActionResult Subscribe(SubscribeModel model);
        IActionResult SubscribeExclusive(SubscribeModel model);
        IActionResult Unsubscribe(SubscribeModel model);
        IActionResult UnsubscribeAll();
    }

    public class SubscribeModel
    {
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }
    }
}

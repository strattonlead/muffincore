//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Muffin.Common.Api.WebSockets;
//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Net.WebSockets;
//using System.Reflection;
//using System.Security.Principal;

//namespace Muffin.Common.Util.WebSockets
//{
//    public abstract class WebSocketsController
//    {
//        #region Properties

//        public HttpRequest Request { get; private set; }
//        public HttpContext HttpContext { get; private set; }
//        public WebSockets WebSockets { get; private set; }
//        public WebSocketsRequest WebSocketsRequest { get; private set; }
//        public WebSocketsResponse WebSocketsResponse { get; private set; }

//        protected readonly IServiceProvider ServiceProvider;
//        public IPrincipal User { get; }

//        #endregion

//        #region Constructor

//        protected WebSocketsController(WebSocketsRequest WebSocketsRequest, WebSocketsResponse WebSocketsResponse, IServiceProvider serviceProvider)
//        {
//            WebSocketsRequest = WebSocketsRequest;
//            Request = WebSocketsRequest?.Context?.Request;
//            WebSockets = WebSocketsRequest?.Socket;
//            HttpContext = WebSocketsRequest?.Context;
//            User = WebSocketsRequest?.Principal;
//            WebSocketsResponse = WebSocketsResponse;
//            ServiceProvider = serviceProvider;
//        }

//        #endregion

//        #region Helper

//        public void RegisterInterface<TInterface, TImplementation>()
//             where TImplementation : new()
//        {
//            WebSocketsRequest?.Handler?.RegisterInterface<TInterface, TImplementation>();
//        }

//        public void RegisterInterfaces(IDictionary<Type, Type> typeMappings)
//        {
//            WebSocketsRequest?.Handler?.RegisterInterfaces(typeMappings);
//        }

//        public virtual bool OnAuthorization(AuthorizationContext context)
//        {
//            if (context?.AllowAnonymous != null)
//            {
//                return true;
//            }

//            var authenticated = context?.Principal?.Identity?.IsAuthenticated;
//            if (authenticated == null || !authenticated.Value)
//            {
//                return false;
//            }

//            if (context?.RequiredRoles != null)
//            {
//                if (context.RequiredRoles.Length > 0)
//                {
//                    foreach (var role in context.RequiredRoles)
//                    {
//                        var isInRole = context?.Principal?.IsInRole(role);
//                        if (isInRole.HasValue && isInRole.Value)
//                        {
//                            return true;
//                        }
//                    }
//                    return false;
//                }
//            }

//            return true;
//        }

//        public virtual void OnActionExecuting() { }

//        public virtual void OnActionExecuted() { }

//        public virtual void OnActionError(Exception exception) { WebSocketsResponse?.SendObjectAsync(exception); }

//        public virtual void SendResponseAsync<T>(Expression<Action<T>> expression)
//        {
//            var apiRequest = ApiRequestHelper.FromExpression(expression, WebSocketsRequest?.RequestId);
//            WebSocketsResponse.SendObjectAsync(apiRequest);
//        }

//        private ApiRequest ApiResponse
//        {
//            get
//            {
//                return new ApiRequest()
//                {
//                    Action = WebSocketsRequest?.Action,
//                    Controller = WebSocketsRequest?.Controller,
//                    RequestId = WebSocketsRequest?.RequestId
//                };
//            }
//        }

//        public virtual void SendResponseAsync()
//        {
//            SendResponseAsync(null);
//        }

//        public virtual void SendResponseAsync<T>(T obj)
//        {
//            SendResponseAsync(new Dictionary<string, T>() { { "model", obj } });
//        }

//        public virtual void SendResponseAsync(IDictionary<string, object> parameters)
//        {
//            var model = ApiResponse;
//            if (parameters != null)
//            {
//                foreach (var parameter in parameters)
//                {
//                    model.SetParam(parameter.Key, parameter.Value);
//                }
//            }
//            WebSocketsResponse.SendObjectAsync(model);
//        }

//        #endregion
//    }

//    public class AuthorizationContext
//    {
//        public IPrincipal Principal { get; set; }
//        public string[] RequiredRoles { get; set; } = new string[0];
//        public MethodInfo TargetMethod { get; set; }
//        public AuthorizeAttribute Authorize { get; set; }
//        public AllowAnonymousAttribute AllowAnonymous { get; set; }
//        public WebSocketsController Sender { get; set; }

//        public AuthorizationContext(IPrincipal principal, MethodInfo targetMethod, WebSocketsController sender)
//        {
//            Principal = principal;
//            TargetMethod = targetMethod;
//            Sender = sender;

//            if (targetMethod != null)
//            {
//                Authorize = targetMethod.GetCustomAttribute<AuthorizeAttribute>(true);
//                if (Authorize == null)
//                {
//                    Authorize = sender?.GetType().GetCustomAttribute<AuthorizeAttribute>(true);
//                }
//                if (Authorize != null)
//                {
//                    RequiredRoles = Authorize.Roles.Split(',');
//                }
//                AllowAnonymous = targetMethod.GetCustomAttribute<AllowAnonymousAttribute>(true);
//            }

//        }
//    }

//    public class AuthorizeRolesAttribute : AuthorizeAttribute
//    {
//        public AuthorizeRolesAttribute(params string[] roles) : base()
//        {
//            Roles = string.Join(",", roles);
//        }
//    }

//    public static class WebSocketsControllerFactory
//    {
//        public static T CreateController<T>(WebSocketsRequest WebSocketsRequest, WebSocketsResponse WebSocketsResponse, IServiceProvider serviceProvider)
//            where T : WebSocketsController
//        {
//            return CreateController(typeof(T), WebSocketsRequest, WebSocketsResponse, serviceProvider) as T;
//        }

//        public static WebSocketsController CreateController(Type type, WebSocketsRequest WebSocketsRequest, WebSocketsResponse WebSocketsResponse, IServiceProvider serviceProvider)
//        {
//            var controller = Activator.CreateInstance(type, WebSocketsRequest, WebSocketsResponse, serviceProvider) as WebSocketsController;
//            return controller;
//        }
//    }
//}

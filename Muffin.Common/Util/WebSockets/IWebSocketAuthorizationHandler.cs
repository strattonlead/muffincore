//using Microsoft.AspNetCore.Http;
//using System;

//namespace Muffin.Common.Util.WebSockets
//{
//    public interface IWebSocketsAuthorizationHandler
//    {
//        bool Authorize(HttpContext context, IServiceProvider serviceProvider);
//    }

//    public class WebSocketsNoneAuthorizationHandler : IWebSocketsAuthorizationHandler
//    {
//        public bool Authorize(HttpContext context, IServiceProvider serviceProvider)
//        {
//            return true;
//        }
//    }

//    public class WebSocketsPrincipalAuthorizationHandler : IWebSocketsAuthorizationHandler
//    {
//        public virtual bool Authorize(HttpContext context, IServiceProvider serviceProvider)
//        {
//            return context.User.Identity != null && context.User.Identity.IsAuthenticated;
//        }
//    }
//}

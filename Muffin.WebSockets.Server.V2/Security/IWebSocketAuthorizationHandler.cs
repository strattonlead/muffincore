using Microsoft.AspNetCore.Http;
using System;

namespace Muffin.WebSockets.Server.Security
{
    public interface IWebSocketAuthorizationHandler
    {
        bool Authorize(HttpContext context, IServiceProvider serviceProvider);
    }

    public class WebSocketNoneAuthorizationHandler : IWebSocketAuthorizationHandler
    {
        public bool Authorize(HttpContext context, IServiceProvider serviceProvider)
        {
            return true;
        }
    }

    public class WebSocketPrincipalAuthorizationHandler : IWebSocketAuthorizationHandler
    {
        public virtual bool Authorize(HttpContext context, IServiceProvider serviceProvider)
        {
            return context.User.Identity != null && context.User.Identity.IsAuthenticated;
        }
    }
}

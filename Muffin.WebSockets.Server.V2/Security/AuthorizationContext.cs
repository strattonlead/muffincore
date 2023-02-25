using Microsoft.AspNetCore.Authorization;
using Muffin.WebSockets.Server.Controllers;
using System.Reflection;
using System.Security.Principal;

namespace Muffin.WebSockets.Server.Security
{
    public class AuthorizationContext
    {
        public IPrincipal Principal { get; set; }
        public string[] RequiredRoles { get; set; } = new string[0];
        public MethodInfo TargetMethod { get; set; }
        public AuthorizeAttribute Authorize { get; set; }
        public AllowAnonymousAttribute AllowAnonymous { get; set; }
        public WebSocketController Sender { get; set; }

        public AuthorizationContext(IPrincipal principal, MethodInfo targetMethod, WebSocketController sender)
        {
            Principal = principal;
            TargetMethod = targetMethod;
            Sender = sender;

            if (targetMethod != null)
            {
                Authorize = targetMethod.GetCustomAttribute<AuthorizeAttribute>(true);
                if (Authorize == null)
                {
                    Authorize = sender?.GetType().GetCustomAttribute<AuthorizeAttribute>(true);
                }
                if (Authorize != null)
                {
                    RequiredRoles = Authorize.Roles?.Split(',');
                }
                AllowAnonymous = targetMethod.GetCustomAttribute<AllowAnonymousAttribute>(true);
            }

        }
    }
}

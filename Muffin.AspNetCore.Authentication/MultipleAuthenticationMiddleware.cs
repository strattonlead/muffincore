using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Muffin.AspNetCore.Authentication
{
    /// <summary>
    /// https://www.mercan.io/2019/08/18/authorize-with-a-multiple-schemes-in-asp-net-core/
    /// </summary>
    public class MultipleAuthenticationMiddleware
    {

        #region Properties

        private readonly RequestDelegate Next;
        private readonly ILogger<MultipleAuthenticationMiddleware> Logger;
        public IAuthenticationSchemeProvider Schemes { get; set; }


        #endregion

        #region Constructor

        public MultipleAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes, ILogger<MultipleAuthenticationMiddleware> logger)
        {
            if (next == null) { throw new ArgumentNullException(nameof(next)); }
            if (schemes == null) { throw new ArgumentNullException(nameof(schemes)); }
            if (logger == null) { throw new ArgumentNullException(nameof(logger)); }
            Logger = logger;
            Next = next;
            Schemes = schemes;
        }

        public MultipleAuthenticationMiddleware(IAuthenticationSchemeProvider schemes)
        {
            Schemes = schemes;
        }

        #endregion

        #region Actions

        public async Task Invoke(HttpContext context)
        {
            context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
            {
                OriginalPath = context.Request.Path,
                OriginalPathBase = context.Request.PathBase
            });

            // Give any IAuthenticationRequestHandler schemes a chance to handle the request
            var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await Schemes.GetAllSchemesAsync())
            {
                var handler = await handlers.GetHandlerAsync(context, scheme.Name) as IAuthenticationRequestHandler;
                try 
                {
                    if (handler != null && await handler.HandleRequestAsync())
                    {
                        return;
                    }

                    var result = await context.AuthenticateAsync(scheme.Name);
                    if (result != null && result.Principal != null && context != null)
                    {
                        context.User = result.Principal;
                        break;
                    }
                }
                catch { }
                
            }
            await Next(context);
        }

        #endregion
    }

    public static class AuthAppBuilderExtensions
    {
        public static IApplicationBuilder UseAllAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            return app.UseMiddleware<MultipleAuthenticationMiddleware>();
        }
    }
}

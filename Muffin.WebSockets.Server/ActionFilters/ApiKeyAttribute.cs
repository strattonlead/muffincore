using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.ActionFilters
{
    /// <summary>
    /// http://codingsonata.com/secure-asp-net-core-web-api-using-api-key-authentication/
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "ApiKey";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key was not provided"
                };
                return;
            }

            var apiKeyProvider = context.HttpContext.RequestServices.GetService<IApiKeyProvider>();
            if (apiKeyProvider == null)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 501,
                    Content = "IApiKeyProvider is not registered"
                };
                return;
            }

            var apiKey = extractedApiKey.ToString();
            apiKeyProvider.GetApiKeys().Contains(apiKey);

            if (!apiKeyProvider.GetApiKeys().Contains(apiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key is not valid"
                };
                return;
            }

            await next();
        }
    }
}

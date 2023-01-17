using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Muffin.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Muffin.Mvc.Helpers
{
    public static class ControllerHelper
    {
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool isPartial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = GetViewEngineResult(controller, viewName, isPartial, viewEngine);

                if (viewResult.Success == false)
                {
                    throw new System.Exception($"A view with the name {viewName} could not be found");
                }

                var viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        private static ViewEngineResult GetViewEngineResult(Controller controller, string viewName, bool isPartial, IViewEngine viewEngine)
        {
            if (viewName.StartsWith("~/"))
            {
                var hostingEnv = controller.HttpContext.RequestServices.GetService(typeof(IHostingEnvironment)) as IHostingEnvironment;
                return viewEngine.GetView(hostingEnv.WebRootPath, viewName, !isPartial);
            }
            else
            {
                return viewEngine.FindView(controller.ControllerContext, viewName, !isPartial);

            }
        }

        public static async Task<string> RenderViewResultToStringAsync(this IActionResult viewResult, IServiceProvider serviceProvider)
        {
            if (viewResult == null) throw new ArgumentNullException(nameof(viewResult));

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var stream = new MemoryStream())
            {
                httpContext.Response.Body = stream; // inject a convenient memory stream
                await viewResult.ExecuteResultAsync(actionContext); // execute view result on that stream

                httpContext.Response.Body.Position = 0;
                return new StreamReader(httpContext.Response.Body).ReadToEnd(); // collect the content of the stream
            }
        }

        public static IActionResult JsonResult(this Controller controller, object obj)
        {
            return controller.JsonResult(obj, null);
        }

        public static IActionResult JsonResult(this Controller controller, object obj, IEnumerable<JsonConverter> jsonConverters)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IdToStringConverter());
            if (jsonConverters?.Any() ?? false)
            {
                foreach (var jsonConverter in jsonConverters)
                {
                    settings.Converters.Add(jsonConverter);
                }
            }
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            if (!controller.Response.Headers.Any(x => x.Key == "Access-Control-Allow-Origin"))
            {
                controller.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(obj, settings),
                ContentType = "application/json",
                StatusCode = 200
            };
        }
    }
}

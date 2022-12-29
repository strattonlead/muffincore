using Microsoft.Extensions.DependencyInjection;
using Muffin.Common.Crypto;
using RazorLight;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Muffin.Services.Razor
{
    public class RazorLightRenderer
    {
        #region Properties

        private readonly RazorLightEngine RazorLightEngine;

        #endregion

        #region Constructor

        public RazorLightRenderer(IServiceProvider serviceProvider)
        {
            RazorLightEngine = serviceProvider.GetRequiredService<RazorLightEngine>();
        }

        #endregion

        #region Helper

        public async Task<string> RenderAsync<T>(string template, T model)
        {
            return await RazorLightEngine.CompileRenderStringAsync(Hash.SHA1(template), template, model);
        }

        #endregion
    }

    public static class RazorlightRendererExtensions
    {
        public static void AddRazorlightRenderer(this IServiceCollection services)
        {
            services.AddScoped(typeof(RazorLightEngine), (serviceProvider) => new RazorLightEngineBuilder()
                      .UseEmbeddedResourcesProject(Assembly.GetEntryAssembly())
                      .UseMemoryCachingProvider()
                      .Build());
            services.AddScoped<RazorLightRenderer>();
        }
    }
}

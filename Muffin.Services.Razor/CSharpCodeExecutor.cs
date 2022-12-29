using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Muffin.Common.Crypto;
using Muffin.Services.Razor.Abstraction;
using RazorLight;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Services.Razor
{
    public class CSharpCodeExecutor : ICSharpCodeExecutor
    {
        #region Properties

        private CancellationTokenSource CancellationTokenSource;

        #endregion

        #region Constructor

        public CSharpCodeExecutor(IApplicationLifetime applicationLifetime)
        {
            CancellationTokenSource = new CancellationTokenSource();

            applicationLifetime.ApplicationStopping.Register(() =>
            {
                CancellationTokenSource.Cancel();
            });
        }

        #endregion

        public async Task<T> Execute<T>(string script)
        {
            return await Execute<T>(script, CancellationTokenSource.Token);
        }

        public async Task<T> Execute<T>(string script, Dictionary<string, object> properties)
        {
            return await Execute<T>(script, properties, CancellationTokenSource.Token);
        }

        public async Task<T> Execute<T>(string script, CancellationToken cancellationToken)
        {
            return await Execute<T>(script, null, cancellationToken);
        }

        public async Task<T> Execute<T>(string script, Dictionary<string, object> properties, CancellationToken cancellationToken)
        {
            if (script == null)
            {
                return default(T);
            }

            return await Task.Run(() =>
            {
                var key = Hash.SHA1(script);


                var engine = new RazorLightEngineBuilder()
                    .UseMemoryCachingProvider()
                    .Build();

                engine.CompileRenderStringAsync(key, script, properties);

                return default(T);
            }, cancellationToken);
        }
    }

    public static class CSharpCodeExecutorHelper
    {
        public static void AddCSharpCodeExecutor(this IServiceCollection services)
        {
            services.AddSingleton<ICSharpCodeExecutor, CSharpCodeExecutor>();
        }
    }
}

using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muffin.Services.V8;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Services.ClearScript
{
    public class ClearScriptExecutor : IJavaScriptExecutor
    {
        #region Properties

        private CancellationTokenSource CancellationTokenSource;
        private readonly ClearScriptExecutorOptions Options;

        #endregion

        #region Constructor

        public ClearScriptExecutor(IServiceProvider serviceProvider)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Options = serviceProvider.GetService<ClearScriptExecutorOptions>();
            var applicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();
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
            return await Execute<T>(script, properties, null, cancellationToken);
        }

        public async Task<T> Execute<T>(string script, Dictionary<string, object> properties, Action<V8ScriptEngine> engineAction, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var flags = V8ScriptEngineFlags.None;
                if (Options != null)
                {
                    flags = Options.V8ScriptEngineFlags;
                }

                using (var engine = new V8ScriptEngine(flags))
                {
                    engineAction?.Invoke(engine);
                    if (Options != null)
                    {
                        foreach (var pair in Options.AdditionalHostTypes)
                        {
                            engine.AddHostObject(pair.Key, pair.Value);
                        }
                    }
                    if (properties != null)
                    {
                        var registeredTypes = new List<Type>();
                        foreach (var property in properties)
                        {
                            engine.Script[property.Key] = property.Value;
                        }
                    }

                    var result = engine.Evaluate(script);
                    return (T)result;
                }
            }, cancellationToken);
        }
    }

    public static class ClearScriptExecutorHelper
    {
        public static void AddClearScriptExecutor(this IServiceCollection services)
        {
            services.AddSingleton<IJavaScriptExecutor, ClearScriptExecutor>();
        }

        public static void AddClearScriptExecutor(this IServiceCollection services, Action<ClearScriptExecutorOptionsBuilder> builder)
        {
            var builderParam = new ClearScriptExecutorOptionsBuilder();
            builder.Invoke(builderParam);
            services.AddSingleton(builderParam.Options);
            services.AddSingleton<IJavaScriptExecutor, ClearScriptExecutor>();
        }
    }

    public class ClearScriptExecutorOptions
    {
        public V8ScriptEngineFlags V8ScriptEngineFlags { get; set; }
        public Dictionary<string, HostTypeCollection> AdditionalHostTypes { get; set; } = new Dictionary<string, HostTypeCollection>();
    }

    public class ClearScriptExecutorOptionsBuilder
    {
        internal ClearScriptExecutorOptions Options = new ClearScriptExecutorOptions();

        public void AddV8ScriptEngineFlags(V8ScriptEngineFlags v8ScriptEngineFlags)
        {
            Options.V8ScriptEngineFlags |= v8ScriptEngineFlags;
        }

        public void AddMscorlibAndSystemCore()
        {
            AddHostTypes("lib", new HostTypeCollection("mscorlib", "System.Core"));
        }

        public void AddHostTypes(string prefix, HostTypeCollection hostTypeCollection)
        {
            Options.AdditionalHostTypes[prefix] = hostTypeCollection;
        }
    }

}

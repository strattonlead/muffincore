using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using V8.Net;

namespace Muffin.Services.V8
{
    public class JavaScriptExecutor : IJavaScriptExecutor
    {
        #region Properties

        private CancellationTokenSource CancellationTokenSource;

        #endregion

        #region Constructor

        public JavaScriptExecutor(IServiceProvider serviceProvider)
        {
            CancellationTokenSource = new CancellationTokenSource();
            var applicationLifetime = serviceProvider.GetService<IApplicationLifetime>();
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
            return await Task.Run(() =>
            {
                using (var engine = new V8Engine(false))
                using (var context = engine.CreateContext())
                {
                    engine.SetContext(context);

                    if (properties != null)
                    {
                        var registeredTypes = new List<Type>();
                        foreach (var property in properties)
                        {
                            var value = property.Value;
                            var propertyType = typeof(object);

                            if (value != null && !value.GetType().IsPrimitive)
                            {
                                propertyType = value.GetType();

                                if (!registeredTypes.Contains(propertyType))
                                {
                                    engine.RegisterType(propertyType, propertyType.Name, true, ScriptMemberSecurity.ReadOnly);
                                    registeredTypes.Add(value.GetType());
                                }
                            }

                            //engine.DynamicGlobalObject[property.Key] = value;


                            var valueHandle = engine.CreateValue(property.Value, true);
                            //engine.GlobalObject.SetProperty(propertyType);
                            engine.GlobalObject.SetProperty(property.Key, valueHandle, V8PropertyAttributes.ReadOnly);
                        }
                    }

                    using (var result = engine.Execute(script))
                    {
                        return result.As<T>();
                    }
                }
            }, cancellationToken);
        }

        public async Task<object> Execute(string script, Type resultType)
        {
            return await Execute(script, resultType, CancellationTokenSource.Token);
        }

        public async Task<object> Execute(string script, Dictionary<string, object> properties, Type resultType)
        {
            return await Execute(script, properties, resultType, CancellationTokenSource.Token);
        }

        public async Task<object> Execute(string script, Type resultType, CancellationToken cancellationToken)
        {
            return await Execute(script, null, resultType, cancellationToken);
        }

        public async Task<object> Execute(string script, Dictionary<string, object> properties, Type resultType, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                using (var engine = new V8Engine(false))
                using (var context = engine.CreateContext())
                {
                    engine.SetContext(context);

                    if (properties != null)
                    {
                        var registeredTypes = new List<Type>();
                        foreach (var property in properties)
                        {
                            var value = property.Value;
                            var propertyType = typeof(object);

                            if (value != null && !value.GetType().IsPrimitive)
                            {
                                propertyType = value.GetType();

                                if (!registeredTypes.Contains(propertyType))
                                {
                                    engine.RegisterType(propertyType, propertyType.Name, true, ScriptMemberSecurity.ReadOnly);
                                    registeredTypes.Add(value.GetType());
                                }
                            }

                            //engine.DynamicGlobalObject[property.Key] = value;


                            var valueHandle = engine.CreateValue(property.Value, true);
                            //engine.GlobalObject.SetProperty(propertyType);
                            engine.GlobalObject.SetProperty(property.Key, valueHandle, V8PropertyAttributes.ReadOnly);
                        }
                    }

                    using (var result = engine.Execute(script))
                    {
                        return result.Value;
                    }
                }
            }, cancellationToken);
        }
    }

    public static class JavaScriptExecutorHelper
    {
        public static void AddJavaScriptExecutor(this IServiceCollection services)
        {
            services.AddSingleton<IJavaScriptExecutor, JavaScriptExecutor>();
        }
    }
}

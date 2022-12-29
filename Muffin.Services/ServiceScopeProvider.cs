using Microsoft.Extensions.DependencyInjection;
using Muffin.Services.Abstraction;
using System;
using System.Collections.Generic;

namespace Muffin.Services
{
    public class ServiceScopeProvider : IServiceScopeProvider
    {
        #region Properties

        private readonly Dictionary<string, Action<IServiceProvider>> _builderActions = new Dictionary<string, Action<IServiceProvider>>();

        #endregion

        #region Actions

        public void PrepareServiceScope(string scopeIdentifier, IServiceScope serviceScope)
        {
            if (_builderActions.TryGetValue(scopeIdentifier, out var action))
            {
                action?.Invoke(serviceScope.ServiceProvider);
            }
        }

        public void RegisterScope<T>(Action<IServiceProvider> scopeBuilder)
        {
            RegisterScope(typeof(T).AssemblyQualifiedName, scopeBuilder);
        }

        public void RegisterScope(string scopeIdentifier, Action<IServiceProvider> scopeBuilder)
        {
            _builderActions[scopeIdentifier] = scopeBuilder;
        }

        #endregion
    }

    public static class ServiceScopeProviderExtensions
    {
        public static void AddServiceScopeProvider(this IServiceCollection services)
        {
            services.AddSingleton<IServiceScopeProvider, ServiceScopeProvider>();
        }

        public static IServiceScope CreateScope<T>(this IServiceScopeFactory serviceScopeFactory)
        {
            return CreateScope(serviceScopeFactory, typeof(T).AssemblyQualifiedName);
        }

        public static IServiceScope CreateScope(this IServiceScopeFactory serviceScopeFactory, string scopeIdentifier)
        {
            var scope = serviceScopeFactory.CreateScope();
            var serviceScopeProvider = scope.ServiceProvider.GetRequiredService<ServiceScopeProvider>();
            serviceScopeProvider.PrepareServiceScope(scopeIdentifier, scope);
            return scope;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Muffin.Services
{
    /// <summary>
    /// Lässt singleton Instanzen mit Scoped Aufrufen zu
    /// </summary>
    /// <typeparam name="T">T muss ein registrierter Service sein. Ansonsten ist er uím Aufruf null</typeparam>
    public class SingletonWithSopedInvocation<T>
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IServiceScopeFactory ServiceScopeFactory;
        public SingletonWithSopedInvocation(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public async Task InvokeScopedAsync(Action<T> action)
        {
            await Task.Run(() =>
            {
                InvokeScoped(action);
            });
        }

        public async Task<TResult> InvokeScopedAsync<TResult>(Func<T, TResult> action)
        {
            return await Task.Run(() =>
            {
                return InvokeScoped(action);
            });
        }

        protected void InvokeScoped(Action<T> action)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                if (typeof(T).IsAssignableFrom(typeof(IServiceProvider)))
                {
                    action?.Invoke((T)scope.ServiceProvider);

                }
                else
                {
                    action?.Invoke(scope.ServiceProvider.GetService<T>());
                }
            }
        }

        protected TResult InvokeScoped<TResult>(Func<T, TResult> action)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                if (action != null)
                {
                    if (typeof(T).IsAssignableFrom(typeof(IServiceProvider)))
                    {
                        return action.Invoke((T)scope.ServiceProvider);
                    }
                    else
                    {
                        return action.Invoke(scope.ServiceProvider.GetService<T>());
                    }
                }
                return default;
            }
        }
    }
}

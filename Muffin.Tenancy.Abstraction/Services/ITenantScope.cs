using Microsoft.Extensions.DependencyInjection;
using Muffin.Tenancy.Abstraction;

namespace Muffin.Tenancy.Services.Abstraction
{
    //public interface ITenantProvider<TKey>
    //{
    //    TKey Id { get; set; }
    //}

    public interface ITenantScope : IDisposable
    {
        void InvokeScoped(ITenant tenant, Action<IServiceScope> tenantScope);
        Task InvokeScopedAsync(ITenant tenant, Func<IServiceScope, Task> tenantScope);
        void InvokeScoped<TScope>(ITenant tenant, Action<TScope> tenantScope)
            where TScope : notnull;
        Task InvokeScopedAsync<TScope>(ITenant tenant, Func<TScope, Task> tenantScope)
            where TScope : notnull;
    }

    public class TenantScope : ITenantScope
    {
        #region Properties

        private readonly IServiceScopeFactory ServiceScopeFactory;

        #endregion

        #region Constructor

        public TenantScope(IServiceProvider serviceProvider)
        {
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        #endregion

        #region ITenantScope

        public void InvokeScoped(ITenant tenant, Action<IServiceScope> tenantScope)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                var scopedTenant = tenantProvider.GetTenant(tenant.Id);
                tenantProvider.ActiveTenant = scopedTenant;

                tenantScope?.Invoke(scope);
                tenantProvider.RestoreTenancy();
            }
        }

        public void InvokeScoped<TScope>(ITenant tenant, Action<TScope> tenantScope)
            where TScope : notnull
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                var scopedTenant = tenantProvider.GetTenant(tenant.Id);
                tenantProvider.ActiveTenant = scopedTenant;

                var serviceScope = scope.ServiceProvider.GetRequiredService<TScope>();
                tenantScope?.Invoke(serviceScope);
                tenantProvider.RestoreTenancy();
            }
        }

        public async Task InvokeScopedAsync(ITenant tenant, Func<IServiceScope, Task> tenantScope)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                var scopedTenant = tenantProvider.GetTenant(tenant.Id);
                tenantProvider.ActiveTenant = scopedTenant;

                var task = tenantScope?.Invoke(scope);
                if (task != null)
                {
                    await task;
                }
                tenantProvider.RestoreTenancy();
            }
        }

        public async Task InvokeScopedAsync<TScope>(ITenant tenant, Func<TScope, Task> tenantScope)
            where TScope : notnull
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                var scopedTenant = tenantProvider.GetTenant(tenant.Id);
                tenantProvider.ActiveTenant = scopedTenant;

                var serviceScope = scope.ServiceProvider.GetRequiredService<TScope>();
                var task = tenantScope?.Invoke(serviceScope);
                if (task != null)
                {
                    await task;
                }
                tenantProvider.RestoreTenancy();
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {

        }

        #endregion
    }

    public static class TenantScopeExtensions
    {
        public static void AddTenantScope(this IServiceCollection services)
        {
            services.AddScoped<ITenantScope, TenantScope>();
        }
    }
}

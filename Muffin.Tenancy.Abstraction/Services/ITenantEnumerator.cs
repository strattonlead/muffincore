using Microsoft.Extensions.DependencyInjection;

namespace Muffin.Tenancy.Abstraction.Services
{
    public interface ITenantEnumerator
    {
        ITenant GetTenant(long id);
        IEnumerable<ITenant> GetTenants(IEnumerable<long> ids);
        IEnumerable<ITenant> GetEnumerator();
    }

    public static class TenantEnumeratorExtensions
    {
        public static void AddTenantEnumerator<TTenantEnumerator>(this IServiceCollection services)
            where TTenantEnumerator : class, ITenantEnumerator
        {
            services.AddScoped<ITenantEnumerator, TTenantEnumerator>();
        }
    }
}

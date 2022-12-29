using Muffin.Tenancy.Abstraction;

namespace Muffin.Tenancy.Services.Abstraction
{
    //public interface ITenantProvider<TKey>
    //{
    //    TKey Id { get; set; }
    //}

    public interface ITenantProvider /*: ITenantProvider<long>*/
    {
        ITenant GetTenant(long id);
        ITenant ActiveTenant { get; set; }
        void RestoreTenancy();
    }


}

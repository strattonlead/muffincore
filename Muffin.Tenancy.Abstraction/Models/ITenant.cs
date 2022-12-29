using Muffin.EntityFrameworkCore.Entity.Abstraction;

namespace Muffin.Tenancy.Abstraction
{
    public interface ITenant<T> : IBaseEntity<T>
    { }

    public interface ITenant : ITenant<long>
    { }

    public interface ITenantOwned
    {
        long? TenantId { get; set; }
    }
}
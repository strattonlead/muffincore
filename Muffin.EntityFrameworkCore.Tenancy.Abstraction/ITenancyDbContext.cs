using Muffin.Tenancy.Services.Abstraction;

namespace Muffin.EntityFrameworkCore.Tenancy.Abstraction
{
    public interface ITenancyDbContext
    {
        ITenantProvider TenantProvider { get; }
        long? TenantId { get; }
    }
}
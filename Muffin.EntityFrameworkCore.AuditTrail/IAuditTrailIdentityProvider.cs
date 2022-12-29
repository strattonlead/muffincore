namespace Muffin.EntityFrameworkCore.AuditTrail
{
    public interface IAuditTrailIdentityProvider
    {
        long? GetIdentity();
    }
}

using System;
using System.Security.Claims;

namespace Muffin.Identity.Abstraction
{
    public interface IIdentityRoleClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
        TKey RoleId { get; set; }
        string ClaimType { get; set; }
        string ClaimValue { get; set; }

        void InitializeFromClaim(Claim other);
        Claim ToClaim();
    }
    public interface IIdentityRoleClaim : IIdentityRoleClaim<long> { }
}

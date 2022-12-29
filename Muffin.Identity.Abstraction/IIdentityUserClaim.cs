using System;
using System.Security.Claims;

namespace Muffin.Identity.Abstraction
{
    public interface IIdentityUserClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
        TKey UserId { get; set; }
        string ClaimType { get; set; }
        string ClaimValue { get; set; }

        void InitializeFromClaim(Claim claim);
        Claim ToClaim();
    }
    public interface IIdentityUserClaim : IIdentityUserClaim<long> { }
}

using System;

namespace Muffin.Identity.Abstraction
{
    public interface IIdentityUserRole<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey UserId { get; set; }
        TKey RoleId { get; set; }
    }

    public interface IIdentityUserRole : IIdentityUserRole<long> { }
}

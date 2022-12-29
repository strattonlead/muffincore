using System;

namespace Muffin.Identity.Abstraction
{
    public interface IIdentityUserLogin<TKey>
        where TKey : IEquatable<TKey>
    {
        string LoginProvider { get; set; }
        string ProviderKey { get; set; }
        string ProviderDisplayName { get; set; }
        TKey UserId { get; set; }
    }

    public interface IIdentityUserLogin : IIdentityUserLogin<long> { }
}

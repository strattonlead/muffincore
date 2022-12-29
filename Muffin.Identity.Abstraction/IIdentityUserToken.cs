using System;

namespace Muffin.Identity.Abstraction
{
    public interface IIdentityUserToken<TKey>
        where TKey : IEquatable<TKey>
        //where TIdentityUser : IEquatable<TKey>
    {
        TKey UserId { get; set; }
        string LoginProvider { get; set; }
        string Name { get; set; }
        string Value { get; set; }
    }

    public interface IIdentityUserToken : IIdentityUserToken<long> { }
}

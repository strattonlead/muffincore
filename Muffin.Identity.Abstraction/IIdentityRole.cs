using System;

namespace Muffin.Identity.Abstraction
{
    public interface IIdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
        string Name { get; set; }
        string NormalizedName { get; set; }
        string ConcurrencyStamp { get; set; }
    }

    public interface IIdentityRole : IIdentityRole<long> { }
}

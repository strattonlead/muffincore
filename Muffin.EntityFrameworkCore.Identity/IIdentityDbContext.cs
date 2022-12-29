using Microsoft.EntityFrameworkCore;
using Muffin.EntityFrameworkCore.Abstraction;
using Muffin.Identity.Abstraction;
using System;

namespace Muffin.EntityFrameworkCore.Identity
{
    public interface IIdentityDbContext : IDbContext { }
    public interface IIdentityDbContext<TKey, TUser> : IIdentityDbContext
        where TKey : IEquatable<TKey>
        where TUser : class, IIdentityUser<TKey>
    {
        DbSet<TUser> IdentityUsers { get; set; }

    }

    public interface IIdentityDbContext<TUser> : IIdentityDbContext<long, TUser>
        where TUser : class, IIdentityUser<long>
    { }

    public interface IIdentityDbContext<TKey, TUser, TUserClaim, TUserLogin, TUserToken, TUserRole, TRole, TRoleClaim> : IIdentityDbContext<TKey, TUser>
        where TKey : IEquatable<TKey>
        where TUser : class, IIdentityUser<TKey>
        where TUserClaim : class, IIdentityUserClaim<TKey>
        where TUserLogin : class, IIdentityUserLogin<TKey>
        where TUserToken : class, IIdentityUserToken<TKey>
        where TUserRole : class, IIdentityUserRole<TKey>
        where TRole : class, IIdentityRole<TKey>
        where TRoleClaim : class, IIdentityRoleClaim<TKey>
    {
        DbSet<TUserClaim> IdentityUserClaims { get; set; }
        DbSet<TUserLogin> IdentityUserLogins { get; set; }
        DbSet<TUserToken> IdentityUserTokens { get; set; }
        DbSet<TUserRole> IdentityUserRoles { get; set; }
        DbSet<TRole> IdentityRoles { get; set; }
        DbSet<TRoleClaim> IdentityRoleClaims { get; set; }
    }

    //public interface IIdentityDbContext : IIdentityDbContext<long, IdentityUser, IdentityUserClaim, IdentityUserLogin, IdentityUserToken, IdentityUserRole, IdentityRole, IdentityRoleClaim>
    //{
    //}
}

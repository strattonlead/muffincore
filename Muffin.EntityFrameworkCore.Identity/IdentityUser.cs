using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.Identity.Abstraction;
using System;
using System.Collections.Generic;

namespace Muffin.EntityFrameworkCore.Identity
{
    public class IdentityUser : IIdentityUser
    {
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string SecurityStamp { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
        public string NormalizedEmail { get; set; }
        public string Email { get; set; }
        public string NormalizedUserName { get; set; }
        public string UserName { get; set; }
        public long Id { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public virtual ICollection<IdentityUserClaim> UserClaims { get; set; }
        public virtual ICollection<IdentityUserLogin> UserLogins { get; set; }
        public virtual ICollection<IdentityUserRole> UserRoles { get; set; }
        public virtual ICollection<IdentityUserToken> UserTokens { get; set; }
    }

    public class IdentityUserTypeConfiguration : IEntityTypeConfiguration<IdentityUser>
    {
        public void Configure(EntityTypeBuilder<IdentityUser> builder)
        {
            builder.HasKey(x => new { x.Id });

            builder.HasMany(x => x.UserClaims)
                .WithOne(x => x.User);

            builder.HasMany(x => x.UserLogins)
                .WithOne(x => x.User);

            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.User);

            builder.HasMany(x => x.UserTokens)
                .WithOne(x => x.User);
        }
    }
}

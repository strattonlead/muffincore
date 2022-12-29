using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.Identity.Abstraction;
using System.Collections.Generic;

namespace Muffin.EntityFrameworkCore.Identity
{

    public class IdentityRole : IIdentityRole
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }
        public virtual ICollection<IdentityRoleClaim> RoleClaims { get; set; }
        public virtual ICollection<IdentityUserRole> UserRoles { get; set; }
    }

    public class IdentityRoleTypeConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasKey(x => new { x.Id });

            builder.HasMany(x => x.RoleClaims)
                .WithOne(x => x.Role);

            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.Role);
        }
    }
}

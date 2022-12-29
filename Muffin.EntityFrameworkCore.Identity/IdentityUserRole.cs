using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.Identity.Abstraction;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Identity
{
    public class IdentityUserRole :  IIdentityUserRole
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public virtual IdentityUser User { get; set; }
        public virtual IdentityRole Role { get; set; }

    }

    public class IdentityUserRoleTypeConfiguration : IEntityTypeConfiguration<IdentityUserRole>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole> builder)
        {
            builder.HasKey(bc => new { bc.UserId, bc.RoleId });
        }
    }
}

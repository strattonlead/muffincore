using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.Identity.Abstraction;
using System.Security.Claims;

namespace Muffin.EntityFrameworkCore.Identity
{
    public class IdentityRoleClaim : IIdentityRoleClaim
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

        public void InitializeFromClaim(Claim other)
        {
            ClaimType = other.Type;
            ClaimValue = other.Value;
        }

        public Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }

        public virtual IdentityRole Role { get; set; }
    }

    public class IdentityRoleClaimTypeConfiguration : IEntityTypeConfiguration<IdentityRoleClaim>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleClaim> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}

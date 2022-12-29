using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.Identity.Abstraction;
using System.Security.Claims;

namespace Muffin.EntityFrameworkCore.Identity
{
    public class IdentityUserClaim : IIdentityUserClaim
    {
        public long Id { get; set; }
        public long UserId { get; set; }
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
        public virtual IdentityUser User { get; set; }
    }

    public class IdentityUserClaimTypeConfiguration : IEntityTypeConfiguration<IdentityUserClaim>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}

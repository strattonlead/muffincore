using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.Identity.Abstraction;

namespace Muffin.EntityFrameworkCore.Identity
{
    public class IdentityUserToken : IIdentityUserToken
    {
        public long UserId { get; set; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public virtual IdentityUser User { get; set; }
    }

    public class IdentityUserTokenTypeConfiguration : IEntityTypeConfiguration<IdentityUserToken>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken> builder)
        {
            builder.HasKey(x => new { x.UserId, x.LoginProvider, x.Value });
        }
    }
}

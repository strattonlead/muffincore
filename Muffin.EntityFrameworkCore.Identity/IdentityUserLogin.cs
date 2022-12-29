using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.Identity.Abstraction;

namespace Muffin.EntityFrameworkCore.Identity
{
    public class IdentityUserLogin : IIdentityUserLogin
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }
        public long UserId { get; set; }
        public virtual IdentityUser User { get; set; }
    }

    public class IdentityUserLoginTypeConfiguration : IEntityTypeConfiguration<IdentityUserLogin>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin> builder)
        {
            builder.HasKey(x => new { x.UserId, x.LoginProvider, x.ProviderKey });
        }
    }
}

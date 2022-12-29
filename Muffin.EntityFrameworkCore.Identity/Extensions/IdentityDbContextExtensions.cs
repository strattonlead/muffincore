using Microsoft.EntityFrameworkCore;

namespace Muffin.EntityFrameworkCore.Identity.Extensions
{
    public static class IdentityDbContextExtensions
    {
        public static void ApplyIdentityConfigurations(this ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(IdentityUser).Assembly);
        }
    }
}
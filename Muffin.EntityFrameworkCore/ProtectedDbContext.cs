using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Muffin.EntityFrameworkCore
{
    public class ProtectedDbContext : DbContext
    {
        #region Properties

        protected readonly IDataProtectionProvider DataProtectionProvider;

        #endregion

        #region Constructor

        public ProtectedDbContext(DbContextOptions options, IDataProtectionProvider dataProtectionProvider)
           : base(options)
        {
            DataProtectionProvider = dataProtectionProvider;
        }

        public ProtectedDbContext(IDataProtectionProvider dataProtectionProvider)
        {
            DataProtectionProvider = dataProtectionProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddDataProtector(DataProtectionProvider);
        }

        #endregion
    }

    public static class ProtectedDbContextExtensions
    {
        public static void AddDataProtector(this ModelBuilder modelBuilder, IDataProtectionProvider dataProtectionProvider)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var attributes = property?.PropertyInfo?.GetCustomAttributes(typeof(ProtectedAttribute), false);
                    if (attributes?.Any() ?? false)
                    {
                        property.SetValueConverter(new ProtectedConverter(dataProtectionProvider));
                    }

                    //attributes = property.PropertyInfo.GetCustomAttributes(typeof(JsonColumnAttribute), false);
                    //if (attributes.Any())
                    //{
                    //    property.SetValueConverter(new JsonPropertyConverter(property.PropertyInfo.PropertyType));
                    //}
                }
            }
        }
    }
}

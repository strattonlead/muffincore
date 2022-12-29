using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Muffin.Common.Util;
using Muffin.EntityFrameworkCore.Tenancy.Abstraction;
using Muffin.Tenancy.Abstraction;
using Muffin.Tenancy.Services.Abstraction;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Muffin.EntityFrameworkCore.Tenancy
{
    public class TenancyDbContext : DbContext, ITenancyDbContext
    {
        #region Properties

        public ITenantProvider TenantProvider { get; }
        public long? TenantId => TenantProvider?.ActiveTenant?.Id;

        #endregion

        #region Constructors

        public TenancyDbContext(IServiceProvider serviceProvider)
            : base(serviceProvider.GetRequiredService<DbContextOptions<TenancyDbContext>>())
        {
            TenantProvider = serviceProvider.GetService<ITenantProvider>();
        }

        #endregion

        #region Helpers

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ConfigureTenantFilter(this);
        }

        #endregion
    }

    internal sealed class TenantModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create([NotNull] DbContext context, bool designTime)
        {
            var tenantProvider = context.GetService<ITenantProvider>();
            return (context.GetType(), tenantProvider?.ActiveTenant?.Id, designTime);
        }
    }

    public static class TenancyDbContextExtensions
    {
        public static void ConfigureTenantModelCacheKeyFactory(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        }

        public static void ConfigureTenantFilter(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        }

        public static void ConfigureTenantFilter(this ModelBuilder builder, ITenancyDbContext dbContext/*, Func<bool> useTenancyFilter = null*/)
        {
            var tenantOwnedTypes = dbContext.GetType()
                .GetProperties()
                .Where(x => x.PropertyType.IsGenericType && typeof(DbSet<>).IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition()))
                .Select(x => x.PropertyType.GetGenericArguments()[0])
                .Where(x => typeof(ITenantOwned).IsAssignableFrom(x))
                .ToArray();

            if (dbContext.TenantProvider != null && dbContext.TenantProvider.ActiveTenant != null)
            {
                foreach (var tenantOwnedType in tenantOwnedTypes)
                {
                    var queryFilter = LambdaHelper.MakeFilterOrNull(tenantOwnedType, nameof(ITenantOwned.TenantId), dbContext.TenantId);
                    builder.Entity(tenantOwnedType).HasQueryFilter(queryFilter);
                }
            }
        }
    }
}
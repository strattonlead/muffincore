using Microsoft.EntityFrameworkCore;
using Muffin.EntityFrameworkCore.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Muffin.EntityFrameworkCore.Globalization
{
    public class GlobalizationDbContext : DbContext, IGlobalizationDbContext
    {
        #region Properties

        public object[] AddedEntries { get; set; }
        public object[] ModifiedEntries { get; set; }
        public object[] DeletedEntries { get; set; }

        #endregion

        #region Constructor

        public GlobalizationDbContext(DbContextOptions<GlobalizationDbContext> options)
            : base(options)
        {
            //this.ConfigureGlobalizationEvents();
        }

        #endregion

        #region DbSets

        public DbSet<LocalizedStringEntity> LocalizedStrings { get; set; }
        public DbSet<LocalizedStringValueEntity> LocalizedStringValues { get; set; }
        public DbSet<LanguageEntity> Languages { get; set; }

        #endregion

        #region Helper


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ConfigureGlobalizationKeys();
        }

        #endregion
    }

    public static class GlobalizationDbContextExtensions
    {
        public static void ConfigureGlobalizationKeys(this ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(LanguageEntity).Assembly);
        }

        public static void ConfigureGlobalizationEvents<T>(this T context)
            where T : DbContext, IGlobalizationDbContext
        {
            context.SavedChanges += SavedChanges;
            context.SavingChanges += SavingChanges;
        }

        private static void SavingChanges(object sender, SavingChangesEventArgs e)
        {
            var context = sender as DbContext;
            var globalizationContext = sender as IGlobalizationDbContext;

            var entries = context.ChangeTracker.Entries();
            globalizationContext.AddedEntries = entries.Where(x => x.State == EntityState.Added).Select(x => x.Entity).ToArray();
            globalizationContext.ModifiedEntries = entries.Where(x => x.State == EntityState.Modified).Select(x => x.Entity).ToArray();
            globalizationContext.DeletedEntries = entries.Where(x => x.State == EntityState.Deleted).Select(x => x.Entity).ToArray();
        }

        private static void SavedChanges(object sender, SavedChangesEventArgs e)
        {
            var context = sender as DbContext;
            var globalizationContext = sender as IGlobalizationDbContext;

            var hasChanges = false;
            if (globalizationContext.AddedEntries != null)
            {
                foreach (BaseEntity addedEntity in globalizationContext.AddedEntries.Where(x => x is BaseEntity))
                {
                    var localizedStringProperties = addedEntity.GetType().GetProperties().Where(x => x.PropertyType == typeof(LocalizedStringEntity) && x.GetCustomAttribute<DbAfterSaveIgnoreAttribute>() == null).ToArray();
                    if (localizedStringProperties.Length > 0)
                    {
                        var keyProperties = addedEntity.GetType().GetProperties().Where(x => x.GetCustomAttribute<KeyAttribute>() != null && x.GetCustomAttribute<NotMappedAttribute>() == null).ToArray();
                        var keySegments = string.Join(".", keyProperties.Select(x => x.GetValue(addedEntity, null)));

                        foreach (var localizedStringProperty in localizedStringProperties)
                        {
                            var key = $"{addedEntity.GetType().Name.ToLower()}.{keySegments}.{localizedStringProperty.Name.ToLower()}";

                            var localizedStringEntity = (LocalizedStringEntity)localizedStringProperty.GetValue(addedEntity);
                            if (localizedStringEntity == null)
                            {
                                localizedStringEntity = new LocalizedStringEntity()
                                {
                                    KeyPath = key
                                };
                                context.Add(localizedStringEntity);
                                hasChanges = true;
                            }
                            else if (localizedStringEntity.KeyPath == null)
                            {
                                localizedStringEntity.KeyPath = key;
                                context.Update(localizedStringEntity);
                                hasChanges = true;
                            }
                        }
                    }
                }
            }

            if (hasChanges)
            {
                context.SaveChanges();
            }
        }
    }
}

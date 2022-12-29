using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.EntityFrameworkCore.AuditTrail
{
    public static class AuditTrailsExtensions
    {
        public static void RegisterAuditTrails(this IAuditTrailContext context, bool usePostAutiting)
        {
            var dbContext = context as DbContext;
            if (dbContext == null)
            {
                throw new ArgumentException($"context must be of type {typeof(DbContext).Name}");
            }

            if (usePostAutiting)
            {
                dbContext.SavedChanges += DbContext_SavedChanges;
                dbContext.SavingChanges += DbContext_PrepareSavedChanges;
            }
            else
            {
                dbContext.SavingChanges += DbContext_SavingChanges;
            }
        }

        private static IEnumerable<AuditEntry> _prepareEntries(IEnumerable<EntityEntry> entries)
        {
            return entries
                .Where(entry => !(entry.Entity is AuditEntity) && entry.State != EntityState.Detached && entry.State != EntityState.Unchanged)
                .Select(x => new AuditEntry(x))
                .ToArray();
        }

        private static void DbContext_PrepareSavedChanges(object? sender, SavingChangesEventArgs e)
        {
            var dbContext = sender as DbContext;
            var cache = dbContext.GetService<AuditTrailCache>();

            dbContext.ChangeTracker.DetectChanges();
            cache.Changes.AddRange(_prepareEntries(dbContext.ChangeTracker.Entries()));
        }

        private static void DbContext_SavedChanges(object? sender, SavedChangesEventArgs e)
        {
            var dbContext = sender as DbContext;
            var cache = dbContext.GetService<AuditTrailCache>();

            if (cache == null)
            {
                throw new ArgumentException($"No service for type {nameof(AuditTrailCache)} registered. Add a Scoped Service. services.AddScoped<{nameof(AuditTrailCache)}>()");
            }

            if (_handleChanges(dbContext, cache.Changes) > 0)
            {
                cache.Changes.Clear();
                dbContext.SaveChanges();
            }
            cache.Changes.Clear();
        }

        private static void DbContext_SavingChanges(object? sender, SavingChangesEventArgs e)
        {
            var dbContext = sender as DbContext;
            dbContext.ChangeTracker.DetectChanges();
            var changes = _prepareEntries(dbContext.ChangeTracker.Entries());
            _handleChanges(dbContext, changes);
        }

        private static int _handleChanges(DbContext dbContext, IEnumerable<AuditEntry> changes)
        {
            var identityProvider = dbContext.GetService<IAuditTrailIdentityProvider>();

            var auditEntries = new List<AuditEntity>();
            foreach (var entry in changes)
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                {
                    continue;
                }

                var auditEntry = new AuditEntity()
                {
                    DateTimeUtc = DateTime.UtcNow,
                    IdentityId = identityProvider?.GetIdentity()
                };

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        break;
                    case EntityState.Deleted:
                        auditEntry.AuditType = AuditType.Delete;
                        break;
                    case EntityState.Modified:
                        auditEntry.AuditType = AuditType.Update;
                        break;
                }

                var modelEntityType = dbContext.Model.FindEntityType(entry.Entity.GetType());
                auditEntry.TableName = modelEntityType?.GetSchemaQualifiedTableName();
#warning TODO events

                var primaryKeys = entry.Properties.Where(x => x.Metadata.IsPrimaryKey()).ToArray();
                if (primaryKeys.Length == 1)
                {
                    auditEntry.SingleId = primaryKeys.FirstOrDefault().CurrentValue as long?;
                }

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.AffectedColumns.Add(propertyName);
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                if (auditEntry.NewValues.Any())
                {
                    auditEntries.Add(auditEntry);
                }
            }

            var auditContext = dbContext as IAuditTrailContext;
            auditContext.Audits.AddRange(auditEntries);

            return auditEntries.Count;
        }
    }

    public class AuditEntry
    {
        public object Entity { get; set; }
        public EntityState State { get; set; }
        public IEnumerable<PropertyEntry> Properties { get; set; }
        public AuditEntry(EntityEntry entity)
        {
            Entity = entity.Entity;
            State = entity.State;
            Properties = entity.Properties.ToArray();
        }
    }
}

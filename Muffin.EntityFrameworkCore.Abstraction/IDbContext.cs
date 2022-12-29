using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Abstraction
{
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        EntityEntry Add([NotNullAttribute] object entity);
        EntityEntry<TEntity> Add<TEntity>([NotNullAttribute] TEntity entity) where TEntity : class;
        void AddRange([NotNullAttribute] IEnumerable<object> entities);
        void AddRange([NotNullAttribute] params object[] entities);

        EntityEntry Remove([NotNullAttribute] object entity);
        EntityEntry<TEntity> Remove<TEntity>([NotNullAttribute] TEntity entity) where TEntity : class;
        void RemoveRange([NotNullAttribute] params object[] entities);
        void RemoveRange([NotNullAttribute] IEnumerable<object> entities);

        EntityEntry<TEntity> Attach<TEntity>([NotNullAttribute] TEntity entity) where TEntity : class;
        EntityEntry Attach([NotNullAttribute] object entity);

        void AttachRange([NotNullAttribute] params object[] entities);
        void AttachRange([NotNullAttribute] IEnumerable<object> entities);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}

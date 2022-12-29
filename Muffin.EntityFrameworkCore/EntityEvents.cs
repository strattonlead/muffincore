using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Muffin.EntityFrameworkCore
{
    public interface IEntitySaveListener { }
    public interface IEntitySaveListener<TContext, TEntity> : IEntitySaveListener
        where TContext : DbContext
    {
        void EntitySaving(TContext context, TEntity entity, EntityState entityState);
        //void EntitySaved(TContext context, TEntity entity);
    }

    public interface IEntityAddListener { }
    public interface IEntityAddListener<TContext, TEntity> : IEntityAddListener
        where TContext : DbContext
    {
        void EntityAdding(TContext context, TEntity entity);
        //void EntityAdded(TContext context, TEntity entity);
    }

    public interface IEntityUpdateListener { }
    public interface IEntityUpdateListener<TContext, TEntity> : IEntityUpdateListener
        where TContext : DbContext
    {
        void EntityUpdateing(TContext context, TEntity entity);
        //void EntityUpdated(TContext context, TEntity entity);
    }

    public interface IEntityDeleteListener { }
    public interface IEntityDeleteListener<TContext, TEntity> : IEntityDeleteListener
        where TContext : DbContext
    {
        void EntityDeleting(TContext context, TEntity entity);
        //void EntityDeleted(TContext context, TEntity entity);
    }

    public static class EntityEventsExentions
    {
        public static void FireSaveEvents<TContext>(this TContext context)
            where TContext : DbContext
        {
            var saved = context.ChangeTracker
                .Entries()
                .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted) && typeof(IEntitySaveListener).IsAssignableFrom(x.Entity.GetType()))
                .ToArray();

            foreach (var item in saved)
            {
                var type = item.Entity.GetType();
                var baseType = typeof(IEntitySaveListener<,>);
                var interfaceType = baseType.MakeGenericType(typeof(TContext), type);
                var mi = interfaceType.GetMethod("EntitySaving");
                mi.Invoke(item.Entity, new object[] { context, item.Entity, item.State });
            }

            var added = context.ChangeTracker
                .Entries()
                .Where(x => x.State == EntityState.Added && typeof(IEntityAddListener).IsAssignableFrom(x.Entity.GetType()))
                .ToArray();

            foreach (var item in added)
            {
                var type = item.Entity.GetType();
                var baseType = typeof(IEntityAddListener<,>);
                var interfaceType = baseType.MakeGenericType(typeof(TContext), type);
                var mi = interfaceType.GetMethod("EntityAdding");
                mi.Invoke(item.Entity, new object[] { context, item.Entity });
            }

            var modified = context.ChangeTracker
                .Entries()
                .Where(x => x.State == EntityState.Modified && typeof(IEntityUpdateListener).IsAssignableFrom(x.Entity.GetType()))
                .ToArray();

            foreach (var item in modified)
            {
                var type = item.Entity.GetType();
                var baseType = typeof(IEntityUpdateListener<,>);
                var interfaceType = baseType.MakeGenericType(typeof(TContext), type);
                var mi = interfaceType.GetMethod("EntityUpdateing");
                mi.Invoke(item.Entity, new object[] { context, item.Entity });
            }

            var deleted = context.ChangeTracker
                .Entries()
                .Where(x => x.State == EntityState.Deleted && typeof(IEntityDeleteListener).IsAssignableFrom(x.Entity.GetType()))
                .ToArray();

            foreach (var item in modified)
            {
                var type = item.Entity.GetType();
                var baseType = typeof(IEntityDeleteListener<,>);
                var interfaceType = baseType.MakeGenericType(typeof(TContext), type);
                var mi = interfaceType.GetMethod("EntityDeleting");
                mi.Invoke(item.Entity, new object[] { context, item.Entity });
            }
        }
    }
}

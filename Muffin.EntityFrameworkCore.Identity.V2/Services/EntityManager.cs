using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Muffin.Identity.Abstraction;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Identity.V2.Services
{
    public class EntityManager
    {
        #region Properties

        protected readonly IServiceScopeFactory ServiceScopeFactory;
        protected readonly IdentityOptions IdentityOptions;
        protected readonly PasswordHasher PasswordHasher;

        #endregion

        #region Constructor

        public EntityManager(IServiceProvider serviceProvider)
        {
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            IdentityOptions = serviceProvider.GetRequiredService<IdentityOptions>();
            PasswordHasher = serviceProvider.GetRequiredService<PasswordHasher>();
        }

        #endregion

        #region Actions

        public class ScopeContext
        {
            public IServiceProvider ServiceProvider { get; set; }
            public IIdentityDbContext DbContext { get; set; }

            public DbSet<IdentityUserLogin> UserLoginSet()
            {
                return _set<IdentityUserLogin>();
            }

            public DbSet<TEntity> RoleSet<TEntity>()
                where TEntity : class, IIdentityRole
            {
                return _set<TEntity>();
            }

            public DbSet<TEntity> UserSet<TEntity>()
                where TEntity : class, IIdentityUser
            {
                return _set<TEntity>();
            }

            private DbSet<TEntity> _set<TEntity>()
                where TEntity : class
            {
                var propertyInfo = DbContext.GetType().GetProperties().FirstOrDefault(x => typeof(TEntity).IsAssignableFrom(x.PropertyType));
                if (propertyInfo != null)
                {
                    return (DbSet<TEntity>)propertyInfo.GetValue(DbContext);
                }
                return null;
            }
        }

        protected string NextConcurrencyStamp
        {
            get
            {
                const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var chars = Enumerable.Range(0, 40).Select(x => pool[new Random().Next(0, pool.Length)]);
                return new string(chars.ToArray());
            }
        }

        public async Task<T> InvokeScopedAsync<T>(Func<ScopeContext, Task<T>> contextFunc)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = (IIdentityDbContext)scope.ServiceProvider.GetRequiredService(IdentityOptions.DbContextType);

                var task = contextFunc?.Invoke(new ScopeContext()
                {
                    ServiceProvider = scope.ServiceProvider,
                    DbContext = dbContext
                });

                if (task != null)
                {
                    return await task;
                }

                return default(T);
            }
        }

        #endregion
    }
}

using Muffin.Identity.Abstraction;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Identity.V2.Services
{
    public class RoleManager<TRole> : EntityManager
        where TRole : class, IIdentityRole
    {
        #region Properties



        #endregion

        #region Constructor

        public RoleManager(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        #endregion

        #region Actions

        public async Task<TRole> CreateRoleAsync(string name)
        {
            return await InvokeScopedAsync(context =>
            {
                var role = context.RoleSet<TRole>().FirstOrDefault(x => x.Name == name);
                if (role != null)
                {
                    return Task.FromResult(role);
                }

                role = Activator.CreateInstance<TRole>();
                role.Name = name;
                role.NormalizedName = name?.ToUpper();
                role.ConcurrencyStamp = NextConcurrencyStamp;

                context.DbContext.Add(role);
                context.DbContext.SaveChanges();

                return Task.FromResult(role);
            });
        }

        #endregion
    }
}

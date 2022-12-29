using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Muffin.Identity.Abstraction;
using System;

namespace Muffin.EntityFrameworkCore.Identity.V2.Services
{
    public static class IdentityExtensions
    {
        public static IdentityOptionsBuilder AddIdentityV2<TUser, TRole>(this IServiceCollection services)
            where TUser : class, IIdentityUser
            where TRole : class, IIdentityRole
        {
            var builder = new IdentityOptionsBuilder(services);
            services.AddSingleton(builder.IdentityOptions);
            services.AddSingleton<PasswordHasher>();
            services.AddSingleton<UserManager<TUser>>();
            services.AddSingleton<RoleManager<TRole>>();
            return builder;
        }
    }

    public class IdentityOptions
    {
        public Type DbContextType { get; set; }
    }

    public class IdentityOptionsBuilder
    {
        #region Properties

        private readonly IServiceCollection Services;
        public IdentityOptions IdentityOptions { get; set; } = new IdentityOptions();

        #endregion

        #region Constructor

        public IdentityOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        #endregion

        #region Actions

        public IdentityOptionsBuilder AddEntityFramework<T>()
           where T : DbContext
        {
            IdentityOptions.DbContextType = typeof(T);
            return this;
        }

        #endregion
    }
}

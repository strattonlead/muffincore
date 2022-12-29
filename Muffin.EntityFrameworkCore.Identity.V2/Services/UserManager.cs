using Microsoft.AspNetCore.Identity;
using Muffin.Identity.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Identity.V2.Services
{
    public class UserManager<TUser> : EntityManager
        where TUser : class, IIdentityUser
    {
        #region Constructor

        public UserManager(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        #endregion

        #region Actions

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            if (user == null)
            {
                return null;
            }

            var userLogins = await InvokeScopedAsync(context =>
            {
                return Task.FromResult(context.UserLoginSet().Where(x => x.UserId == user.Id).ToArray());
            });

            return userLogins
                .Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))
                .ToList();
        }

        public async Task<IdentityResult> AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "", Description = "" });
            }

            var userLogin = await InvokeScopedAsync<IdentityUserLogin>(context =>
            {
                var userLogin = context
                    .UserLoginSet()
                    .FirstOrDefault(x => x.UserId == user.Id
                        && x.LoginProvider == login.LoginProvider
                        && x.ProviderKey == login.ProviderKey
                        && x.ProviderDisplayName == login.ProviderDisplayName);
                if (userLogin == null)
                {
                    userLogin = new IdentityUserLogin()
                    {
                        UserId = user.Id,
                        LoginProvider = login.LoginProvider,
                        ProviderDisplayName = login.ProviderDisplayName,
                        ProviderKey = login.ProviderKey
                    };
                    context.DbContext.Add(userLogin);
                    context.DbContext.SaveChanges();
                }
                return Task.FromResult(userLogin);
            });

            if (userLogin == null)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "", Description = "" });
            }
            return IdentityResult.Success;
        }

        public async Task<TUser> FindByNameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return await InvokeScopedAsync(context => Task.FromResult(context.UserSet<TUser>().FirstOrDefault(x => x.UserName == username)));
        }

        public async Task<TUser> GetUserAsync(ClaimsPrincipal principal)
        {
            var username = principal?.Identity?.Name;
            return await FindByNameAsync(username);
        }

        public async Task<bool> CheckPasswordAsync(TUser user, string password)
        {
            return await Task.Run(() =>
            {
                if (user == null || password == null)
                {
                    return false;
                }

                return PasswordHasher.ComputeSecureHash(password) == user.PasswordHash;
            });
        }

        public async Task<IdentityResult> DeleteAsync(TUser user)
        {
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "", Description = "" });
            }

            await InvokeScopedAsync<object>(context =>
            {
                context.DbContext.Attach(user);
                context.DbContext.Remove(user);
                context.DbContext.SaveChanges();
                return null;
            });

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateAsync(TUser user)
        {
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "", Description = "" });
            }

            return await CreateAsync(user.UserName, user.Email, null);
        }

        public async Task<IdentityResult> CreateAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return IdentityResult.Failed(new IdentityError() { Code = "", Description = "" });
            }

            var user = await InvokeScopedAsync(context =>
            {
                var user = context.UserSet<TUser>().FirstOrDefault(x => x.UserName == username);
                if (user == null)
                {
                    user = Activator.CreateInstance<TUser>();
                    user.UserName = username;
                    user.NormalizedUserName = username.ToUpper();

                    user.Email = email;
                    user.NormalizedEmail = email?.ToUpper();

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        user.PasswordHash = PasswordHasher.ComputeSecureHash(password);
                    }

                    context.DbContext.Add(user);
                    context.DbContext.SaveChanges();
                }

                return Task.FromResult(user);
            });

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "", Description = "" });
            }

            return IdentityResult.Success;
        }

        #endregion
    }

    //public class IdentityResult<TUser>
    //    where TUser : class, IIdentityUser
    //{
    //    #region Properties

    //    public bool Success { get; set; }
    //    public TUser User { get; set; }

    //    #endregion

    //    #region Constructor

    //    public IdentityResult(TUser user)
    //    {
    //        Success = user != null;
    //        User = user;
    //    }

    //    #endregion
    //}
}

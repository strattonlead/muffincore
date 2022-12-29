//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.DependencyInjection;
//using Muffin.Identity.Abstraction;
//using System;
//using System.Collections.Generic;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace Muffin.EntityFrameworkCore.Identity.V2.Services
//{
//    public class SignInManager<TUser> :EntityManager
//        where TUser : class, IIdentityUser
//    {
//        #region Properties

//        private readonly IAuthenticationSchemeProvider AuthenticationSchemeProvider;
//        private readonly IHttpContextAccessor HttpContextAccessor;
//        private readonly UserManager<TUser> UserManager;
//        private HttpContext HttpContext { get => HttpContextAccessor.HttpContext; }

//        #endregion

//        #region Constructor

//        public SignInManager(IServiceProvider serviceProvider)
//            :base (serviceProvider)
//        {
//            AuthenticationSchemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
//            HttpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
//            UserManager = serviceProvider.GetRequiredService<UserManager<TUser>>();
//        }

//        #endregion

//        #region Actions

//        public async Task<ClaimsIdentity> CreateUserIdentityAsync(TUser user)
//        {
//            return UserManager.CreateIdentityAsync(user, AuthenticationType);
//        }

//        public async Task SignInAsync(TUser user, AuthenticationProperties authenticationProperties, string authenticationMethod = null) {

//        }

//        public async Task SignInAsync(TUser user, bool isPersistent, string authenticationMethod = null) {
//            var userIdentity = await CreateUserIdentityAsync(user).WithCurrentCulture();
//            // Clear any partial cookies from external or two factor partial sign ins
//            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
//            if (rememberBrowser)
//            {
//                var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(ConvertIdToString(user.Id));
//                HttpContext.SignInAsync(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity, rememberBrowserIdentity);
//            }
//            else
//            {
//                HttpContext.SignInAsync(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
//            }
//        }

//        public async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims) {

//        }

//        public async Task SignInWithClaimsAsync(TUser user, bool isPersistent, IEnumerable<Claim> additionalClaims) { 

//        }

//        public async Task SignOutAsync() {
//            var schemes = await AuthenticationSchemeProvider.GetAllSchemesAsync();
//            foreach (var scheme in schemes)
//            {
//                await HttpContext.SignOutAsync(scheme.Name);
//            }
//        }

//        #endregion
//    }
//}

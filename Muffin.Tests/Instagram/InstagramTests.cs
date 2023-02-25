using CreateIf.Instagram.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muffin.Primes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.Tests.Instagram
{
    [TestClass]
    public class InstagramTests
    {
        private IServiceProvider ServiceProvider;
        private IInstagramOAuth2Service InstagramService => ServiceProvider.GetRequiredService<IInstagramOAuth2Service>();

        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();
            services.AddInstagramOAuth2Service(options =>
            {
                options.UseAppId("6066764640069422");
                options.UseAppSecret("c7b147b6a82248bb37e50fa77fa47f46");
                options.UseRedirectUri("https://localhost/auth");
                options.UseUserProfileScope();
            });
            ServiceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public void TestGetAuthUrl()
        {
            var authUrl = InstagramService.GetAuthUrl();
            var result = "https://api.instagram.com/oauth/authorize?client_id=6066764640069422&redirect_uri=https://localhost/auth&scope=user_profile&response_type=code";
            Assert.AreEqual(result, authUrl);
        }

        [TestMethod]
        public void TestCodeToTokenExchange()
        {
            var code = "AQAruNe6T-f0S13CsRMeOXKa2vC7I6sh8njg97GznG55Ivvw1llhnX049ppvCAQJPGleHal1saD9sUqhhEfTNgjcg_DJ81A--XYMwDkebfXxEjE7DvzKmAd5nGzRLq7nRHvLefM3x18g0ZLrmVOGHR0uCBTlRa1SrjcLC_2WvJaAJx6K7l4E4sMhuQkitdCbFcx009gGnRj54rYyg3hfRUJhcX_yw_pN-QWusM8J87SHAA";
            var accessToken = InstagramService.GetAccessToken(code).Result;
            //var accessToken = InstagramService.GetLongLivedUserAccessToken(code).Result;
        }

        [TestMethod]
        public void TestCodeToLongLivedCode()
        {
            var code = "IGQVJWSTMyOFVRX3hpd2VPY2JMdlZAIZAXFHM3BVSVlDcVZAvZAUZASS3ZAJdUpNX20wUGdKb0JZAZAG1GV2lyR0thdGlJWmR1Q3ZAocklENW1iTVlOSkJVUjFPc29CU1NlMVlnNHNBdmpGYW14V1Y2U1l4WUNXaERVZA050Mkx4OVk0";
            var accessToken = InstagramService.GetLongLivedUserAccessToken(code).Result;
        }

    }
}

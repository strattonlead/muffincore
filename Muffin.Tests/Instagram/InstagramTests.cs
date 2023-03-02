using CreateIf.Instagram.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Muffin.Tests.Instagram
{
    [TestClass]
    public class InstagramTests
    {
        private IServiceProvider ServiceProvider;
        private IInstagramOAuth2Service InstagramOAuthService => ServiceProvider.GetRequiredService<IInstagramOAuth2Service>();
        private IInstagramService InstagramService => ServiceProvider.GetRequiredService<IInstagramService>();

        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();
            services.AddInstagramOAuth2Service(options =>
            {
                options.UseAppId("199008449400766");
                options.UseAppSecret("cfe7e827e888ef5949fa3b95313d1258");
                options.UseRedirectUri("https://localhost/auth");
                options.UseUserProfileScope();
            });
            services.AddInstagramService();
            ServiceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public void TestGetAuthUrl()
        {
            var authUrl = InstagramOAuthService.GetAuthUrl();
            var result = "https://api.instagram.com/oauth/authorize?client_id=6066764640069422&redirect_uri=https://localhost/auth&scope=user_profile&response_type=code";
            Assert.AreEqual(result, authUrl);
        }

        [TestMethod]
        public void TestCodeToTokenExchange()
        {
            var code = "AQADpXuBAxr-XqNJM75p-oPJn0gvuvmDnnwgNNgDyucDbUTylVy19tgi00SnxpTj633icDqI1wfn7PthiPnKt3ivc1E8LA-lHwe_7fzN-ASTCmqfv3mTjkPwBVJuHZnaD3T7ozYmiAyK1aTrlv0bxb0UZzJr9vlNVvdQwF9T_d3h4Gq8_su40X9kcGueAB2hEFtBss3bW-zrs1vqazRBbOZKgvIzOGELOR74WlyP6nFMPg";
            var accessToken = InstagramOAuthService.GetAccessToken(code).Result;
            //var accessToken = InstagramService.GetLongLivedUserAccessToken(code).Result;
        }

        [TestMethod]
        public void TestCodeToLongLivedCode()
        {
            var code = "IGQVJWd01wZAExyTDV0SnZA1ZATB1X3dENHlkQmU4b1pCdGsyN1l1cUh0aWZAlMEdjbU9hRlRXNEl6YTVUNHhGSVVHVkh4SFd3Y1oxNFdIM3d2djVpTlQzSTNlUTNNakFzLWZAIeXVFQWg2bkRlZAkxIbFFoQ1V4Uk9MOGprVms0";
            var token = new UserAccessTokenResponse()
            {
                AccessToken = code,
                UserId = "17841432364306647"
            };
            var accessToken = InstagramOAuthService.GetLongLivedUserAccessToken(token).Result;
        }

        [TestMethod]
        public void TestInstaSevice()
        {
            var token = new UserAccessTokenResponse()
            {
                AccessToken = "IGQVJWLTh1SllLTlJtdnFBU1FUeGt6WGlZAZAEh4Qm9sVlVDY1p2eEEwRnlGMThKbXhYTlhBeW1UcVdabEIxWU5SZA19jTEs0bURZASHZAsWHZABeVlkTzkwNV9IbEItenc3aU50T29mN0UxaC01OXlkQjZAqNgZDZD",
                //AccessToken = "IGQVJVVFh2ZADNWVWR1QlJBY0ZALMVpMRG4yRUMtbktaU2lyN2ZAORGtFZAWxTNFlIWk9zeVBOUzRBUHFFZAUN4eVg1aHMwU212VktUcXhmQkJaTjI3SEhiM29zYzhGVGtJcUYxZAjJ0ZAG5udjh5Vk9Gb3E2OAZDZD",
                //AccessToken = "IGQVJYZAzNrT3FJWTJIOWNhdjJTRVVLalNaaTl5N2J4bTFDbmtCTURkYWFvbTNvWV9SSnZAiQnhWTHpOZAkYzTk1kUGlVYzRqMGtNUU5vWG55WlFLNUg2R2U3QS1lNUdWREVNZAVlaUGdn",

                UserId = "17841432364306647"
            };
            var me = InstagramService.Me(token).Result;
            var bd = InstagramService.BusinessDiscovery(token, "alune.collection").Result;
        }
    }
}

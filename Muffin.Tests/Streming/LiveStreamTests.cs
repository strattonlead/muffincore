//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using PuppeteerSharp;
//using System;
//using System.Threading.Tasks;

//namespace Muffin.Tests.Streming
//{
//    [TestClass]
//    public class LiveStreamTests
//    {
//        [TestMethod]
//        public async Task StreamTest()
//        {
//            //using var browserFetcher = new BrowserFetcher();
//            //await browserFetcher.DownloadAsync();
//            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
//            {
//                Headless = false,
//                IgnoreHTTPSErrors = true,
//                //Args = new string[] { "--proxy-server=\"socks5://proxy-server.scraperapi.com:8080\"" },
//                Args = new string[] { "--proxy-server=\"socks5://184.178.172.18:15280\"" },
//                ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"
//            });
//            await using var page = await browser.NewPageAsync();
//            await page.AuthenticateAsync(new Credentials() { Username = "scraperapi", Password = "e66c422a422c9b8a112626072d4db0dd" });
//            //await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0");
//            //await page.GoToAsync("https://www.twitch.tv/chill_and_study_beats"/*, new NavigationOptions() { Referer = "https://google.com" }*/);
//            await page.GoToAsync("https://google.de"/*, new NavigationOptions() { Referer = "https://google.com" }*/);
//            //await page.WaitForSelectorAsync("[data-a-target=\"consent-banner-accept\"]");
//            //await page.ClickAsync("[data-a-target=\"consent-banner-accept\"]");


//            await Task.Delay(TimeSpan.FromSeconds(60));
//            await browser.CloseAsync();
//        }
//    }
//}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muffin.SevDesk.Api;
using SevDeskClient;
using System;
using System.Net;

namespace Muffin.Tests.SevDesk
{
    [TestClass]
    public class SevDeskTests
    {
        private IServiceProvider ServiceProvider;
        private ISevDeskService SevDeskService => ServiceProvider.GetRequiredService<ISevDeskService>();

        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();
            services.AddSevDeskService(options => options.UseToken("9ec2833379aa14f3fff5452af2cceddd"));
            ServiceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public void TestServiceProvider()
        {
            Assert.IsNotNull(SevDeskService);
        }

        [TestMethod]
        public void TestInvoiceFunctions()
        {
            var invoiceListResult = SevDeskService.GetListAsync<Invoice>().Result;
            Assert.AreEqual(HttpStatusCode.OK, invoiceListResult.StatusCode);

            if (invoiceListResult.StatusCode == HttpStatusCode.OK && invoiceListResult.Result.Count > 0)
            {
                var invoice = invoiceListResult.Result[0];
                var invoiceResult = SevDeskService.GetAsync<Invoice>(invoice.Id).Result;

                Assert.AreEqual(HttpStatusCode.OK, invoiceResult.StatusCode);
                Assert.AreEqual(invoice.invoiceNumber, invoiceResult.Result.invoiceNumber);

                var streamResult = SevDeskService.GetPdfAsync(invoice).Result;
                Assert.AreEqual(HttpStatusCode.OK, streamResult.StatusCode);
            }
        }
    }
}

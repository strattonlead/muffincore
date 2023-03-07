using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muffin.SevDesk.Api;
using SevDeskClient;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // Token vom Test Account arndt.bieberstein@gmail.com
            services.AddSevDeskService(options => options.UseToken("f6275bb005abe83ec3d9a442cac2c367"));
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

        [TestMethod]
        public void TestCreateInvoiceWithContact()
        {
            //            Supplier(ID: 2)
            //Customer(ID: 3)
            //Partner(ID: 4)
            //Prospect Customer(ID: 28)

            var getContactListResult = SevDeskService.GetListAsync<Contact>().Result;
            Assert.AreEqual(HttpStatusCode.OK, getContactListResult.StatusCode);

            var name = "UnitTest";
            var contact = getContactListResult.Result.FirstOrDefault(x => x.Name == name);
            if (contact == null)
            {
                contact = new Contact()
                {
                    Name = "UnitTest",
                    Category = Categories.Customer
                };
                var addContactResult = SevDeskService.AddAsync(contact).Result;
                Assert.AreEqual(HttpStatusCode.OK, addContactResult.StatusCode);
                contact = addContactResult.Result;
            }

            var getContactResult = SevDeskService.GetAsync<Contact>(contact.Id).Result;
            Assert.AreEqual(HttpStatusCode.OK, getContactResult.StatusCode);
            contact = getContactResult.Result;

            // "{\"objects\":null,\"error\":{\"message\":\"invoice expected array with 'id' and 'objectName'. array given\",\"code\":null,\"data\":null,\"exceptionUUID\":\"9a3153b2-3205-417a-8307-672f07469222\"}}"

            var invoice = new Invoice()
            {
                currency = "EUR",
                contact = contact,
                invoiceDate = DateTime.UtcNow.Date,
                createUser = null
            };
            var invoiceItems = new List<InvoicePos>();
            invoiceItems.Add(new InvoicePos()
            {
                invoice = invoice,
                quantity = "1",
                priceNet = "15.00"
            });
            var result = SevDeskService.FactorySaveInvoiceAsync(invoice, invoiceItems).Result;
        }
    }
}

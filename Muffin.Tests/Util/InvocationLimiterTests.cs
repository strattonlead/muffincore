using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muffin.Common.Util;
using System;
using System.Threading.Tasks;

namespace Muffin.Tests.Util
{
    [TestClass]
    public class InvocationLimiterTests
    {
        [TestMethod]
        public async Task PerMinuteDelayTest()
        {
            var limiter = new InvocationLimiter()
            {
                InvocationsPerMinute = 60
            };

            var start = DateTime.UtcNow;
            var end = start.AddSeconds(9);

            var cnt = 0;
            while (end > DateTime.UtcNow)
            {
                await limiter.InvokeAsync(() => { cnt++; Task.Delay((int)(new Random().NextDouble() * 840) + 150).Wait(); });
            }

            Assert.AreEqual(10, cnt);
        }

        [TestMethod]
        public async Task PerHourDelayTest()
        {
            var limiter = new InvocationLimiter()
            {
                InvocationsPerHour = 3600
            };

            var start = DateTime.UtcNow;
            var end = start.AddSeconds(9);

            var cnt = 0;
            while (end > DateTime.UtcNow)
            {
                await limiter.InvokeAsync(() => { cnt++; Task.Delay((int)(new Random().NextDouble() * 840) + 150).Wait(); });
            }

            Assert.AreEqual(10, cnt);
        }

        [TestMethod]
        public async Task PerMinuteAndHourDelayTest()
        {
            var limiter = new InvocationLimiter()
            {
                InvocationsPerMinute = 60,
                InvocationsPerHour = 8000
            };

            var start = DateTime.UtcNow;
            var end = start.AddSeconds(9);

            var cnt = 0;
            while (end > DateTime.UtcNow)
            {
                await limiter.InvokeAsync(() => { cnt++; Task.Delay((int)(new Random().NextDouble() * 840) + 150).Wait(); });
            }

            Assert.AreEqual(10, cnt);
        }

        [TestMethod]
        public async Task PerHourAndMinuteDelayTest()
        {
            var limiter = new InvocationLimiter()
            {
                InvocationsPerMinute = 120,
                InvocationsPerHour = 3600
            };

            var start = DateTime.UtcNow;
            var end = start.AddSeconds(10);

            var cnt = 0;
            while (end > DateTime.UtcNow)
            {
                await limiter.InvokeAsync(() => { cnt++; Task.Delay((int)(new Random().NextDouble() * 840) + 150).Wait(); });
            }

            Assert.AreEqual(10, cnt);
        }
    }
}

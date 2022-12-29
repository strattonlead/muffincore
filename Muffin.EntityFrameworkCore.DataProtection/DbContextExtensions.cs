using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Muffin.EntityFrameworkCore.DataProtection
{
    public interface IDataProtectionDbContext
    {
        IDataProtector DataProtector { get; set; }
    }

    public static class DbContextExtensions
    {
        /// <summary>
        /// Call this method inside the DbContext Constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="protectorName"></param>
        public static void AddDataProtection<T>(this T context, IServiceProvider serviceProvider, string protectorName)
            where T : IDataProtectionDbContext
        {
            context.DataProtector = serviceProvider.GetRequiredService<IDataProtectionProvider>().CreateProtector(protectorName);

        }

        public static void AddDataProtection<T>(this T context, IServiceProvider serviceProvider)
           where T : IDataProtectionDbContext
        {
            context.AddDataProtection(serviceProvider, typeof(T).Name);
        }
    }
}

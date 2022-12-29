using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace Muffin.EntityFrameworkCore.DataProtection
{
    //[AttributeUsage(AttributeTargets.Property)]
    //public class ProtectedAttribute : Attribute { }

    public static class DbContextDataProtection
    {
        /// <summary>
        /// builder.Entity<SmtpAccountEntity>(b =>
        ///{
        ///    b.AddProtecedDataConverters(DataProtector);
        ///});
        /// </summary>
        public static void AddProtecedDataConverters<TEntity>(this EntityTypeBuilder<TEntity> b, IDataProtector dataProtector)
        where TEntity : class
        {
            var protectedProps = typeof(TEntity).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(ProtectedAttribute)));

            foreach (var p in protectedProps)
            {
                //if (p.PropertyType != typeof(string))
                //{
                //    var converterType = typeof(ProtectedDataConverter<>)
                //        .MakeGenericType(p.PropertyType);
                //    var converter = (ValueConverter)Activator
                //        .CreateInstance(converterType, dataProtector);

                //    b.Property(p.PropertyType, p.Name).HasConversion(converter);
                //}
                //else
                //{
                var converter = new ProtectedDataConverter(dataProtector);
                b.Property(typeof(string), p.Name).HasConversion(converter);
                //}
            }
        }

        private class ProtectedDataConverter : ValueConverter<string, string>
        {
            public ProtectedDataConverter(IDataProtectionProvider protectionProvider)
                : base(
                        s => protectionProvider
                            .CreateProtector("personal_data")
                            .Protect(s),
                        s => protectionProvider
                            .CreateProtector("personal_data")
                            .Unprotect(s),
                        default)
            {
            }
        }

        //private class ProtectedDataConverter<T> : ValueConverter<T, string>
        //{
        //    public ProtectedDataConverter(IDataProtector dataProtector)
        //        : base(s => dataProtector
        //                    .Protect(JsonSerializer.Serialize(s)),
        //                s => JsonSerializer.Deserialize<T>(
        //                    protectionProvider.CreateProtector("personal_data")
        //                    .Unprotect(s)),
        //                default)
        //    {
        //    }
        //}
    }
}

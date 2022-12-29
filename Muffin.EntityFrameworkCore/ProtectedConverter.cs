using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;

namespace Muffin.EntityFrameworkCore
{
    public class ProtectedConverter : ValueConverter<string, string>
    {
        class Wrapper
        {
            readonly IDataProtector _dataProtector;

            public Wrapper(IDataProtectionProvider dataProtectionProvider)
            {
                _dataProtector = dataProtectionProvider.CreateProtector(nameof(ProtectedConverter));
            }

            public Expression<Func<string, string>> To => x => x != null ? _dataProtector.Protect(x) : null;
            public Expression<Func<string, string>> From => x => x != null ? _dataProtector.Unprotect(x) : null;
        }

        public ProtectedConverter(IDataProtectionProvider provider, ConverterMappingHints mappingHints = default)
            : this(new Wrapper(provider), mappingHints)
        { }

        ProtectedConverter(Wrapper wrapper, ConverterMappingHints mappingHints)
            : base(wrapper.To, wrapper.From, mappingHints)
        { }
    }
}
